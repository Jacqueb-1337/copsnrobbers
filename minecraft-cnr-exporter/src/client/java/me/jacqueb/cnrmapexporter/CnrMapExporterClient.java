package me.jacqueb.cnrmapexporter;

import com.google.gson.Gson;
import com.mojang.blaze3d.vertex.PoseStack;
import com.mojang.blaze3d.vertex.VertexConsumer;
import com.mojang.brigadier.arguments.StringArgumentType;
import net.fabricmc.api.ClientModInitializer;
import net.fabricmc.fabric.api.client.command.v2.ClientCommandManager;
import net.fabricmc.fabric.api.client.command.v2.ClientCommandRegistrationCallback;
import net.fabricmc.fabric.api.client.event.lifecycle.v1.ClientTickEvents;
import net.fabricmc.fabric.api.client.rendering.v1.WorldRenderEvents;
import net.minecraft.client.Minecraft;
import net.minecraft.client.renderer.ItemBlockRenderTypes;
import net.minecraft.client.renderer.LevelRenderer;
import net.minecraft.client.renderer.MultiBufferSource;
import net.minecraft.client.renderer.RenderType;
import net.minecraft.client.renderer.block.model.BakedQuad;
import net.minecraft.client.resources.model.BakedModel;
import net.minecraft.client.renderer.texture.TextureAtlasSprite;
import net.minecraft.core.BlockPos;
import net.minecraft.core.Direction;
import net.minecraft.network.chat.Component;
import net.minecraft.resources.ResourceLocation;
import net.minecraft.tags.BlockTags;
import net.minecraft.util.RandomSource;
import net.minecraft.world.entity.decoration.ArmorStand;
import net.minecraft.world.level.BlockAndTintGetter;
import net.minecraft.world.level.ColorResolver;
import net.minecraft.world.level.Level;
import net.minecraft.server.level.ServerLevel;
import net.minecraft.world.level.biome.Biome;
import net.minecraft.world.level.block.Block;
import net.minecraft.world.level.block.Blocks;
import net.minecraft.world.level.block.entity.BlockEntity;
import net.minecraft.world.level.block.state.BlockState;
import net.minecraft.world.level.chunk.ChunkAccess;
import net.minecraft.world.level.chunk.LevelChunkSection;
import net.minecraft.world.level.chunk.status.ChunkStatus;
import net.minecraft.world.level.lighting.LevelLightEngine;
import net.minecraft.world.level.material.FluidState;
import net.minecraft.world.level.material.Fluids;
import net.minecraft.world.phys.BlockHitResult;
import net.minecraft.world.phys.HitResult;
import net.minecraft.world.phys.AABB;
import net.minecraft.world.phys.Vec3;
import net.minecraft.world.phys.shapes.VoxelShape;

import javax.imageio.ImageIO;
import java.awt.Graphics2D;
import java.awt.image.BufferedImage;
import java.io.BufferedWriter;
import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.*;

public final class CnrMapExporterClient implements ClientModInitializer {
    private static final int CHUNK_SIZE = 16;
    private static final int MAX_VERTICES_PER_PART = 60000;
    private static final float PACKED_POSITION_SCALE = 1024f;
    private static final float PACKED_UV_SCALE = 65535f;
    private static BlockPos pos1;
    private static BlockPos pos2;
    private static ExportJob activeExport;

    @Override
    public void onInitializeClient() {
        ClientCommandRegistrationCallback.EVENT.register((dispatcher, registryAccess) -> dispatcher.register(
            ClientCommandManager.literal("cnr")
                .then(ClientCommandManager.literal("pos1")
                    .executes(ctx -> setPos(ctx.getSource().getClient(), true))
                    .then(ClientCommandManager.argument("x", StringArgumentType.word())
                        .then(ClientCommandManager.argument("y", StringArgumentType.word())
                            .then(ClientCommandManager.argument("z", StringArgumentType.word()).executes(ctx ->
                                setPos(ctx.getSource().getClient(), true,
                                    StringArgumentType.getString(ctx, "x"),
                                    StringArgumentType.getString(ctx, "y"),
                                    StringArgumentType.getString(ctx, "z")))))))
                .then(ClientCommandManager.literal("pos2")
                    .executes(ctx -> setPos(ctx.getSource().getClient(), false))
                    .then(ClientCommandManager.argument("x", StringArgumentType.word())
                        .then(ClientCommandManager.argument("y", StringArgumentType.word())
                            .then(ClientCommandManager.argument("z", StringArgumentType.word()).executes(ctx ->
                                setPos(ctx.getSource().getClient(), false,
                                    StringArgumentType.getString(ctx, "x"),
                                    StringArgumentType.getString(ctx, "y"),
                                    StringArgumentType.getString(ctx, "z")))))))
                .then(ClientCommandManager.literal("clear").executes(ctx -> {
                    pos1 = null; pos2 = null;
                    ctx.getSource().sendFeedback(Component.literal("CNR selection cleared."));
                    return 1;
                }))
                .then(ClientCommandManager.literal("export")
                    .then(ClientCommandManager.argument("name", StringArgumentType.word()).executes(ctx -> {
                        String name = StringArgumentType.getString(ctx, "name");
                        return exportSelection(ctx.getSource().getClient(), name, ctx.getSource()::sendFeedback);
                    })))
                .then(ClientCommandManager.literal("cancel").executes(ctx -> cancelExport(ctx.getSource().getClient())))
                .then(ClientCommandManager.literal("status").executes(ctx -> exportStatus(ctx.getSource().getClient())))
        ));
        WorldRenderEvents.AFTER_ENTITIES.register(CnrMapExporterClient::renderSelectionOutline);
        ClientTickEvents.END_CLIENT_TICK.register(CnrMapExporterClient::tickExportJob);
    }

    private static int setPos(Minecraft client, boolean first) {
        if (client.player == null || client.level == null) return 0;
        BlockPos p = targetBlock(client);
        return setPos(client, first, p);
    }

    private static int setPos(Minecraft client, boolean first, String x, String y, String z) {
        if (client.player == null || client.level == null) return 0;
        BlockPos base = client.player.blockPosition();
        try {
            BlockPos p = new BlockPos(
                parseCoordinate(x, base.getX()),
                parseCoordinate(y, base.getY()),
                parseCoordinate(z, base.getZ()));
            return setPos(client, first, p);
        } catch (NumberFormatException ex) {
            client.player.displayClientMessage(Component.literal("Invalid coordinates. Use numbers or relative values like ~, ~5, ~-3."), false);
            return 0;
        }
    }

    private static int setPos(Minecraft client, boolean first, BlockPos p) {
        if (first) pos1 = p.immutable(); else pos2 = p.immutable();
        client.player.displayClientMessage(Component.literal("CNR " + (first ? "pos1" : "pos2") + " = " + p.getX() + ", " + p.getY() + ", " + p.getZ()), false);
        return 1;
    }

    private static int parseCoordinate(String raw, int base) {
        String value = raw == null ? "" : raw.trim();
        if (value.startsWith("~")) {
            String delta = value.substring(1);
            return (int)Math.floor(base + (delta.isEmpty() ? 0.0 : Double.parseDouble(delta)));
        }
        return (int)Math.floor(Double.parseDouble(value));
    }

    private static void renderSelectionOutline(net.fabricmc.fabric.api.client.rendering.v1.WorldRenderContext context) {
        if (pos1 == null || pos2 == null || context == null) return;
        PoseStack matrices = context.matrixStack();
        MultiBufferSource consumers = context.consumers();
        if (matrices == null || consumers == null) return;

        Vec3 camera = context.camera().getPosition();
        double minX = Math.min(pos1.getX(), pos2.getX()) - camera.x;
        double minY = Math.min(pos1.getY(), pos2.getY()) - camera.y;
        double minZ = Math.min(pos1.getZ(), pos2.getZ()) - camera.z;
        double maxX = Math.max(pos1.getX(), pos2.getX()) + 1.0 - camera.x;
        double maxY = Math.max(pos1.getY(), pos2.getY()) + 1.0 - camera.y;
        double maxZ = Math.max(pos1.getZ(), pos2.getZ()) + 1.0 - camera.z;
        VertexConsumer lines = consumers.getBuffer(RenderType.lines());
        LevelRenderer.renderLineBox(matrices, lines, new AABB(minX, minY, minZ, maxX, maxY, maxZ), 1.0f, 0.35f, 0.1f, 1.0f);
    }

    private static BlockPos targetBlock(Minecraft client) {
        HitResult hit = client.hitResult;
        if (hit instanceof BlockHitResult bhr && hit.getType() == HitResult.Type.BLOCK) return bhr.getBlockPos();
        return client.player.blockPosition();
    }

    private interface Feedback { void send(Component text); }

    private static int exportSelection(Minecraft mc, String rawName, Feedback feedback) {
        if (mc.level == null || mc.player == null) return 0;
        if (pos1 == null || pos2 == null) {
            feedback.send(Component.literal("Set /cnr pos1 and /cnr pos2 first."));
            return 0;
        }
        if (activeExport != null) {
            feedback.send(Component.literal("A CNR export is already running. Use /cnr status or /cnr cancel."));
            return 0;
        }

        String name = sanitizeName(rawName);
        BlockPos min = new BlockPos(Math.min(pos1.getX(), pos2.getX()), Math.min(pos1.getY(), pos2.getY()), Math.min(pos1.getZ(), pos2.getZ()));
        BlockPos max = new BlockPos(Math.max(pos1.getX(), pos2.getX()), Math.max(pos1.getY(), pos2.getY()), Math.max(pos1.getZ(), pos2.getZ()));
        long volume = (long)(max.getX()-min.getX()+1) * (max.getY()-min.getY()+1) * (max.getZ()-min.getZ()+1);
        activeExport = new ExportJob(mc, min, max, name);
        feedback.send(Component.literal("CNR export queued: " + volume + " block selection volume. The game will stay responsive; use /cnr status or /cnr cancel."));
        return 1;
    }

    private static int cancelExport(Minecraft mc) {
        if (activeExport == null) {
            sendClientMessage(mc, "No CNR export is running.", false);
            return 0;
        }
        if (activeExport.finalizing) {
            sendClientMessage(mc, "CNR export is already finalizing the output file and cannot be cancelled safely.", false);
            return 0;
        }
        activeExport.cancelled = true;
        sendClientMessage(mc, "Cancelling CNR export...", false);
        return 1;
    }

    private static int exportStatus(Minecraft mc) {
        if (activeExport == null) {
            sendClientMessage(mc, "No CNR export is running.", false);
            return 0;
        }
        sendClientMessage(mc, activeExport.statusText(), false);
        return 1;
    }

    private static void tickExportJob(Minecraft mc) {
        ExportJob job = activeExport;
        if (job == null) return;
        job.tick(mc);
        if (job.done && activeExport == job) activeExport = null;
    }

    private static void sendClientMessage(Minecraft mc, String text, boolean actionBar) {
        if (mc != null && mc.player != null) mc.player.displayClientMessage(Component.literal(text), actionBar);
    }

    private static String sanitizeName(String s) {
        String n = s == null ? "minecraft_map" : s.replaceAll("[^A-Za-z0-9_-]", "_");
        return n.isBlank() ? "minecraft_map" : n;
    }

    private static final class ExportJob {
        private static final long CLIENT_WORK_BUDGET_NS = 4_000_000L;
        private static final long PROGRESS_INTERVAL_NS = 750_000_000L;

        final Level initialClientLevel;
        final ExportContext ctx;
        final int minChunkX, maxChunkX, minChunkZ, maxChunkZ;
        final int chunkColumns;
        final long totalChunks;

        long nextChunkIndex;
        long completedChunks;
        int currentBlockIndex;
        long lastProgressNanos;
        boolean cancelled;
        boolean done;
        boolean finalizing;
        volatile boolean finalizationDone;
        volatile boolean snapshotPending;
        volatile ChunkSnapshot readySnapshot;
        volatile Throwable asyncError;
        volatile Throwable finalizationError;
        volatile Path outputPath;
        ChunkSnapshot currentSnapshot;

        ExportJob(Minecraft mc, BlockPos min, BlockPos max, String name) {
            this.initialClientLevel = mc.level;
            this.ctx = new ExportContext(mc, min, max, name);
            this.minChunkX = Math.floorDiv(min.getX(), 16);
            this.maxChunkX = Math.floorDiv(max.getX(), 16);
            this.minChunkZ = Math.floorDiv(min.getZ(), 16);
            this.maxChunkZ = Math.floorDiv(max.getZ(), 16);
            this.chunkColumns = maxChunkX - minChunkX + 1;
            this.totalChunks = (long)chunkColumns * (maxChunkZ - minChunkZ + 1L);
        }

        void tick(Minecraft mc) {
            if (done) return;
            if (cancelled) {
                sendClientMessage(mc, "CNR export cancelled after " + completedChunks + "/" + totalChunks + " chunks.", false);
                done = true;
                return;
            }
            if (mc.level == null || mc.level != initialClientLevel) {
                fail(mc, new IllegalStateException("World changed while export was running."));
                return;
            }
            if (asyncError != null) {
                fail(mc, asyncError);
                return;
            }

            if (finalizing) {
                if (finalizationDone) {
                    if (finalizationError != null) fail(mc, finalizationError);
                    else {
                        sendClientMessage(mc, "CNR map exported: " + outputPath.toAbsolutePath(), false);
                        sendClientMessage(mc, "Non-air blocks=" + ctx.nonAirBlockCount + " chunks=" + ctx.chunks.size() + " textures=" + ctx.textureCountAtFinalize + " quads=" + ctx.renderQuadCount + " collision boxes=" + ctx.collisionBoxCount + " bullet-pass boxes=" + ctx.bulletPassThroughBoxCount + " climbable boxes=" + ctx.climbableBoxCount + " Cops spawns=" + ctx.copSpawns.size() + " Robbers spawns=" + ctx.robberSpawns.size(), false);
                        done = true;
                    }
                } else {
                    maybeShowProgress(mc, "CNR export: finalizing and writing " + ctx.name + ".json...");
                }
                return;
            }

            if (currentSnapshot == null && readySnapshot != null) {
                currentSnapshot = readySnapshot;
                readySnapshot = null;
                snapshotPending = false;
                currentBlockIndex = 0;
                if (ctx.nonAirBlockCount + currentSnapshot.blocks.size() > ExportContext.MAX_NON_AIR_BLOCKS) {
                    fail(mc, new IllegalStateException("Selection contains more than 4,000,000 non-air blocks."));
                    return;
                }
                ctx.acceptSpawnMarkers(currentSnapshot.markers);
            }

            long deadline = System.nanoTime() + CLIENT_WORK_BUDGET_NS;
            while (currentSnapshot != null && currentBlockIndex < currentSnapshot.blocks.size()) {
                BlockEntry entry = currentSnapshot.blocks.get(currentBlockIndex++);
                try {
                    ctx.processBlock(entry, currentSnapshot.view);
                } catch (Throwable t) {
                    fail(mc, t);
                    return;
                }
                if ((currentBlockIndex & 31) == 0 && System.nanoTime() >= deadline) break;
            }

            if (currentSnapshot != null && currentBlockIndex >= currentSnapshot.blocks.size()) {
                completedChunks++;
                nextChunkIndex++;
                currentSnapshot = null;
                currentBlockIndex = 0;
            }

            if (nextChunkIndex >= totalChunks && currentSnapshot == null && readySnapshot == null && !snapshotPending) {
                startFinalization(mc);
                return;
            }

            if (currentSnapshot == null && readySnapshot == null && !snapshotPending && nextChunkIndex < totalChunks) {
                requestNextSnapshot(mc);
            }

            maybeShowProgress(mc, statusText());
        }

        void requestNextSnapshot(Minecraft mc) {
            int cx = minChunkX + (int)(nextChunkIndex % chunkColumns);
            int cz = minChunkZ + (int)(nextChunkIndex / chunkColumns);
            snapshotPending = true;

            if (ctx.sourceLevel instanceof ServerLevel serverLevel && ctx.sourceLevel != mc.level) {
                serverLevel.getServer().execute(() -> {
                    if (cancelled || done) return;
                    try {
                        readySnapshot = ChunkSnapshot.capture(ctx, serverLevel, cx, cz, true);
                    } catch (Throwable t) {
                        asyncError = t;
                    }
                });
            } else {
                try {
                    readySnapshot = ChunkSnapshot.capture(ctx, mc.level, cx, cz, false);
                } catch (Throwable t) {
                    asyncError = t;
                }
            }
        }

        void startFinalization(Minecraft mc) {
            finalizing = true;
            sendClientMessage(mc, "CNR export scan complete. Finalizing " + ctx.name + ".json in the background...", false);
            Path outDir = mc.gameDirectory.toPath().resolve("cnr_exports");
            Thread writerThread = new Thread(() -> {
                try {
                    ctx.textureCountAtFinalize = ctx.textures.size();
                    ctx.atlas = Atlas.pack(ctx.textures.values());
                    ctx.textures.clear();
                    Files.createDirectories(outDir);
                    Path out = outDir.resolve(ctx.name + ".json");
                    try (BufferedWriter writer = Files.newBufferedWriter(out)) {
                        new Gson().toJson(ctx.finish(), writer);
                    }
                    outputPath = out;
                } catch (Throwable t) {
                    finalizationError = t;
                } finally {
                    finalizationDone = true;
                }
            }, "CNR-Map-Exporter-Writer");
            writerThread.setDaemon(true);
            writerThread.start();
        }

        String statusText() {
            double chunkProgress = completedChunks;
            if (currentSnapshot != null && !currentSnapshot.blocks.isEmpty())
                chunkProgress += Math.min(1.0, currentBlockIndex / (double)currentSnapshot.blocks.size());
            int percent = totalChunks <= 0 ? 100 : (int)Math.min(100, Math.floor(chunkProgress * 100.0 / totalChunks));
            return "CNR export " + percent + "% | chunks " + completedChunks + "/" + totalChunks + " | non-air " + ctx.nonAirBlockCount;
        }

        void maybeShowProgress(Minecraft mc, String text) {
            long now = System.nanoTime();
            if (now - lastProgressNanos < PROGRESS_INTERVAL_NS) return;
            lastProgressNanos = now;
            sendClientMessage(mc, text, true);
        }

        void fail(Minecraft mc, Throwable t) {
            t.printStackTrace();
            String message = t.getMessage();
            sendClientMessage(mc, "CNR export failed: " + t.getClass().getSimpleName() + (message == null ? "" : ": " + message), false);
            done = true;
        }
    }

    private static final class BlockEntry {
        final BlockPos pos;
        final BlockState state;
        BlockEntry(BlockPos pos, BlockState state) { this.pos = pos; this.state = state; }
    }

    private static final class SpawnMarker {
        final UUID id;
        final String team;
        final float[] position;
        SpawnMarker(UUID id, String team, float[] position) {
            this.id = id; this.team = team; this.position = position;
        }
    }

    private static final class ChunkSnapshot {
        final List<BlockEntry> blocks;
        final List<SpawnMarker> markers;
        final SnapshotView view;

        ChunkSnapshot(List<BlockEntry> blocks, List<SpawnMarker> markers, SnapshotView view) {
            this.blocks = blocks; this.markers = markers; this.view = view;
        }

        static ChunkSnapshot capture(ExportContext ctx, Level level, int cx, int cz, boolean allowLoad) {
            ChunkAccess chunk;
            if (allowLoad) {
                chunk = level.getChunk(cx, cz);
            } else {
                chunk = level.getChunkSource().getChunk(cx, cz, ChunkStatus.FULL, false);
                if (chunk == null)
                    throw new IllegalStateException("Selection includes an unloaded multiplayer chunk at " + cx + ", " + cz + ". Load the selected area before exporting.");
            }

            int baseX = cx << 4;
            int baseZ = cz << 4;
            int x0 = Math.max(ctx.min.getX(), baseX);
            int x1 = Math.min(ctx.max.getX(), baseX + 15);
            int z0 = Math.max(ctx.min.getZ(), baseZ);
            int z1 = Math.min(ctx.max.getZ(), baseZ + 15);
            int y0 = Math.max(ctx.min.getY(), chunk.getMinBuildHeight());
            int y1 = Math.min(ctx.max.getY(), chunk.getMinBuildHeight() + chunk.getHeight() - 1);

            List<BlockEntry> blocks = new ArrayList<>();
            List<SpawnMarker> markers = new ArrayList<>();
            Map<Long, BlockState> states = new HashMap<>();
            Map<Long, Biome> biomes = new HashMap<>();
            Biome defaultBiome = null;

            if (x0 <= x1 && y0 <= y1 && z0 <= z1) {
                LevelChunkSection[] sections = chunk.getSections();
                int chunkMinY = chunk.getMinBuildHeight();
                for (int si = 0; si < sections.length; si++) {
                    LevelChunkSection section = sections[si];
                    int sectionY0 = chunkMinY + (si << 4);
                    int sectionY1 = sectionY0 + 15;
                    if (section == null || section.hasOnlyAir() || sectionY1 < y0 || sectionY0 > y1) continue;
                    int sy0 = Math.max(y0, sectionY0);
                    int sy1 = Math.min(y1, sectionY1);
                    for (int y = sy0; y <= sy1; y++) {
                        int ly = y - sectionY0;
                        for (int z = z0; z <= z1; z++) {
                            int lz = z - baseZ;
                            for (int x = x0; x <= x1; x++) {
                                BlockState state = section.getBlockState(x - baseX, ly, lz);
                                if (state.isAir()) continue;
                                BlockPos pos = new BlockPos(x, y, z);
                                states.put(pos.asLong(), state);
                                Biome biome = level.getBiome(pos).value();
                                if (defaultBiome == null) defaultBiome = biome;
                                biomes.put(pos.asLong(), biome);
                                blocks.add(new BlockEntry(pos, state));
                            }
                        }
                    }
                }

                captureHorizontalBorder(level, ctx, states, baseX - 1, z0, z1, y0, y1, true);
                captureHorizontalBorder(level, ctx, states, baseX + 16, z0, z1, y0, y1, true);
                captureHorizontalBorder(level, ctx, states, baseZ - 1, x0, x1, y0, y1, false);
                captureHorizontalBorder(level, ctx, states, baseZ + 16, x0, x1, y0, y1, false);

                AABB bounds = new AABB(x0, y0, z0, x1 + 1.0, y1 + 1.0, z1 + 1.0);
                for (ArmorStand stand : level.getEntitiesOfClass(ArmorStand.class, bounds)) {
                    if (stand == null || !stand.hasCustomName()) continue;
                    Component customName = stand.getCustomName();
                    String marker = customName == null ? "" : customName.getString().trim();
                    if (!marker.equalsIgnoreCase("Cops") && !marker.equalsIgnoreCase("Robbers")) continue;
                    markers.add(new SpawnMarker(stand.getUUID(), marker, new float[] {
                        (float)(stand.getX() - ctx.min.getX()),
                        (float)(stand.getY() - ctx.min.getY()),
                        (float)(stand.getZ() - ctx.min.getZ())
                    }));
                }
            }

            return new ChunkSnapshot(blocks, markers,
                new SnapshotView(states, biomes, defaultBiome, chunk.getMinBuildHeight(), chunk.getHeight()));
        }

        static void captureHorizontalBorder(Level level, ExportContext ctx, Map<Long, BlockState> states,
                                            int fixed, int range0, int range1, int y0, int y1, boolean fixedX) {
            if (fixedX && (fixed < ctx.min.getX() || fixed > ctx.max.getX())) return;
            if (!fixedX && (fixed < ctx.min.getZ() || fixed > ctx.max.getZ())) return;
            for (int y = y0; y <= y1; y++) {
                for (int v = range0; v <= range1; v++) {
                    BlockPos pos = fixedX ? new BlockPos(fixed, y, v) : new BlockPos(v, y, fixed);
                    BlockState state = level.getBlockState(pos);
                    if (!state.isAir()) states.put(pos.asLong(), state);
                }
            }
        }
    }

    private static final class SnapshotView implements BlockAndTintGetter {
        final Map<Long, BlockState> states;
        final Map<Long, Biome> biomes;
        final Biome defaultBiome;
        final int minBuildHeight;
        final int height;

        SnapshotView(Map<Long, BlockState> states, Map<Long, Biome> biomes, Biome defaultBiome, int minBuildHeight, int height) {
            this.states = states; this.biomes = biomes; this.defaultBiome = defaultBiome;
            this.minBuildHeight = minBuildHeight; this.height = height;
        }

        @Override public BlockEntity getBlockEntity(BlockPos pos) { return null; }
        @Override public BlockState getBlockState(BlockPos pos) {
            BlockState state = states.get(pos.asLong());
            return state == null ? Blocks.AIR.defaultBlockState() : state;
        }
        @Override public FluidState getFluidState(BlockPos pos) { return getBlockState(pos).getFluidState(); }
        @Override public int getHeight() { return height; }
        @Override public int getMinBuildHeight() { return minBuildHeight; }
        @Override public float getShade(Direction direction, boolean shade) { return 1.0f; }
        @Override public LevelLightEngine getLightEngine() { return null; }
        @Override public int getBlockTint(BlockPos pos, ColorResolver resolver) {
            Biome biome = biomes.get(pos.asLong());
            if (biome == null) biome = defaultBiome;
            return biome == null ? 0xFFFFFF : resolver.getColor(biome, pos.getX(), pos.getZ());
        }

        int getWaterTint(BlockPos pos) {
            Biome biome = biomes.get(pos.asLong());
            if (biome == null) biome = defaultBiome;
            return biome == null ? 0x3F76E4 : biome.getWaterColor();
        }
    }

    private static final class ExportContext {
        static final long MAX_NON_AIR_BLOCKS = 4_000_000L;
        final Minecraft mc;
        final Level sourceLevel;
        final BlockPos min, max;
        final String name;
        final Map<String, TextureDef> textures = new LinkedHashMap<>();
        final Map<ChunkKey, ChunkBuilder> chunks = new LinkedHashMap<>();
        final List<float[]> copSpawns = new ArrayList<>();
        final List<float[]> robberSpawns = new ArrayList<>();
        final Set<UUID> seenSpawnMarkers = new HashSet<>();
        int textureCountAtFinalize;
        long nonAirBlockCount;
        long renderQuadCount;
        long collisionBoxCount;
        long bulletPassThroughBoxCount;
        long climbableBoxCount;
        Atlas atlas;

        ExportContext(Minecraft mc, BlockPos min, BlockPos max, String name) {
            this.mc = mc; this.min = min; this.max = max; this.name = name;
            Level level = mc.level;
            try {
                if (mc.hasSingleplayerServer() && mc.getSingleplayerServer() != null && mc.level != null) {
                    ServerLevel serverLevel = mc.getSingleplayerServer().getLevel(mc.level.dimension());
                    if (serverLevel != null) level = serverLevel;
                }
            } catch (Throwable ignored) { }
            this.sourceLevel = level;
        }

        void processBlock(BlockEntry entry, SnapshotView view) throws Exception {
            nonAirBlockCount++;
            int lx = entry.pos.getX() - min.getX();
            int ly = entry.pos.getY() - min.getY();
            int lz = entry.pos.getZ() - min.getZ();
            ChunkBuilder chunk = chunkFor(lx, ly, lz);
            boolean bulletPassThrough = entry.state.is(Blocks.BARRIER);
            if (!bulletPassThrough) {
                emitRenderModel(entry.state, entry.pos, lx, ly, lz, chunk, view);
                FluidState fluid = entry.state.getFluidState();
                if (fluid != null && !fluid.isEmpty())
                    emitFluid(fluid, entry.pos, lx, ly, lz, chunk, view);
            }
            emitCollision(entry.state, entry.pos, lx, ly, lz, chunk, view,
                bulletPassThrough ? chunk.bulletPassThrough : chunk.collision, bulletPassThrough);
            emitClimbable(entry.state, entry.pos, lx, ly, lz, chunk, view);
        }

        void acceptSpawnMarkers(List<SpawnMarker> markers) {
            for (SpawnMarker marker : markers) {
                if (!seenSpawnMarkers.add(marker.id)) continue;
                if (marker.team.equalsIgnoreCase("Cops")) copSpawns.add(marker.position);
                else if (marker.team.equalsIgnoreCase("Robbers")) robberSpawns.add(marker.position);
            }
        }

        ChunkBuilder chunkFor(int x, int y, int z) {
            int cx = Math.floorDiv(x, CHUNK_SIZE) * CHUNK_SIZE;
            int cy = Math.floorDiv(y, CHUNK_SIZE) * CHUNK_SIZE;
            int cz = Math.floorDiv(z, CHUNK_SIZE) * CHUNK_SIZE;
            ChunkKey key = new ChunkKey(cx,cy,cz);
            return chunks.computeIfAbsent(key, k -> new ChunkBuilder(k));
        }

        void emitRenderModel(BlockState state, BlockPos worldPos, int lx, int ly, int lz, ChunkBuilder chunk, SnapshotView view) throws Exception {
            BakedModel model = mc.getBlockRenderer().getBlockModel(state);
            String layer = renderLayer(state);
            long seed = state.getSeed(worldPos);

            for (Direction dir : Direction.values()) {
                BlockPos neighbor = worldPos.relative(dir);
                if (!Block.shouldRenderFace(state, view, worldPos, dir, neighbor)) continue;
                RandomSource random = RandomSource.create(seed);
                for (BakedQuad q : model.getQuads(state, dir, random)) emitQuad(q, state, worldPos, lx,ly,lz, chunk, layer, view);
            }
            RandomSource random = RandomSource.create(seed);
            for (BakedQuad q : model.getQuads(state, null, random)) emitQuad(q, state, worldPos, lx,ly,lz, chunk, layer, view);
        }

        void emitFluid(FluidState fluid, BlockPos worldPos, int lx, int ly, int lz, ChunkBuilder chunk, SnapshotView view) {
            boolean water = isWater(fluid);
            boolean lava = isLava(fluid);
            if (!water && !lava) return;

            int tint = water ? view.getWaterTint(worldPos) : 0xFFFFFF;
            ResourceLocation spriteId = ResourceLocation.withDefaultNamespace(water ? "block/water_still" : "block/lava_still");
            String texId = spriteId + "#tint_" + String.format("%06x", tint & 0xFFFFFF);
            final int bakedTint = tint & 0xFFFFFF;
            textures.computeIfAbsent(texId, id -> loadStandaloneTexture(spriteId, texId, bakedTint));

            float x0 = lx - chunk.key.x;
            float y0 = ly - chunk.key.y;
            float z0 = lz - chunk.key.z;
            float x1 = x0 + 1f;
            float z1 = z0 + 1f;
            float height = fluid.getHeight(view, worldPos);
            if (height <= 0.001f || height > 1f) height = 1f;
            float y1 = y0 + height;
            if (water) chunk.water.addBox(x0, y0, z0, x1, y1, z1);

            RenderBuilder target = chunk.transparent;
            if (!sameFluidKind(view.getFluidState(worldPos.above()), water, lava))
                addFluidQuad(target, texId, new float[]{x0,y1,z0, x0,y1,z1, x1,y1,z1, x1,y1,z0});
            if (!sameFluidKind(view.getFluidState(worldPos.north()), water, lava))
                addFluidQuad(target, texId, new float[]{x0,y0,z0, x0,y1,z0, x1,y1,z0, x1,y0,z0});
            if (!sameFluidKind(view.getFluidState(worldPos.south()), water, lava))
                addFluidQuad(target, texId, new float[]{x1,y0,z1, x1,y1,z1, x0,y1,z1, x0,y0,z1});
            if (!sameFluidKind(view.getFluidState(worldPos.west()), water, lava))
                addFluidQuad(target, texId, new float[]{x0,y0,z1, x0,y1,z1, x0,y1,z0, x0,y0,z0});
            if (!sameFluidKind(view.getFluidState(worldPos.east()), water, lava))
                addFluidQuad(target, texId, new float[]{x1,y0,z0, x1,y1,z0, x1,y1,z1, x1,y0,z1});
        }

        boolean isWater(FluidState fluid) {
            return fluid != null && (fluid.getType() == Fluids.WATER || fluid.getType() == Fluids.FLOWING_WATER);
        }

        boolean isLava(FluidState fluid) {
            return fluid != null && (fluid.getType() == Fluids.LAVA || fluid.getType() == Fluids.FLOWING_LAVA);
        }

        boolean sameFluidKind(FluidState fluid, boolean water, boolean lava) {
            return (water && isWater(fluid)) || (lava && isLava(fluid));
        }

        void addFluidQuad(RenderBuilder target, String texId, float[] p) {
            float[] uv = new float[]{0f,0f, 0f,1f, 1f,1f, 1f,0f};
            target.quads.add(new QuadDef(p, uv, texId));
            renderQuadCount++;

            // Liquids need to remain visible from inside the volume as well. Export the
            // reverse winding too instead of changing the shared transparent material's
            // culling behavior for every glass/cutout surface in a DLC map.
            float[] back = new float[]{
                p[9],p[10],p[11], p[6],p[7],p[8], p[3],p[4],p[5], p[0],p[1],p[2]
            };
            target.quads.add(new QuadDef(back, uv, texId));
            renderQuadCount++;
        }

        int fallbackMinecraftTint(ResourceLocation spriteId) {
            String path = spriteId == null ? "" : spriteId.getPath().toLowerCase(Locale.ROOT);
            if (path.contains("water")) return 0x3F76E4;
            if (path.contains("lily_pad")) return 0x208030;
            if (path.contains("spruce_leaves")) return 0x619961;
            if (path.contains("birch_leaves")) return 0x80A755;
            if (path.contains("leaves") || path.contains("vine")) return 0x77AB2F;
            if (path.contains("grass") || path.contains("fern")) return 0x91BD59;
            return 0xFFFFFF;
        }

        void emitQuad(BakedQuad q, BlockState state, BlockPos worldPos, int lx, int ly, int lz, ChunkBuilder chunk, String layer, SnapshotView view) throws Exception {
            int[] packed = q.getVertices();
            if (packed == null || packed.length < 20 || packed.length % 4 != 0) return;
            int stride = packed.length / 4;
            TextureAtlasSprite sprite = q.getSprite();
            ResourceLocation spriteId = sprite.contents().name();
            int tint = 0xFFFFFF;
            if (q.isTinted()) {
                int resolved = -1;
                try {
                    // Resolve against the captured biome-aware snapshot. Using the live/server
                    // Level here can return no tint while export work is running off the render
                    // path, which is how Raid ended up with #tint_ffffff vegetation.
                    resolved = mc.getBlockColors().getColor(state, view, worldPos, q.getTintIndex());
                } catch (Throwable ignored) { }
                tint = resolved >= 0 ? (resolved & 0xFFFFFF) : fallbackMinecraftTint(spriteId);
            }
            String texId = q.isTinted() ? (spriteId + "#tint_" + String.format("%06x", tint)) : spriteId.toString();
            final int bakedTint = tint;
            textures.computeIfAbsent(texId, id -> loadTexture(spriteId, sprite, texId, bakedTint));

            float[] p = new float[12];
            float[] uv = new float[8];
            float u0 = sprite.getU0(), u1 = sprite.getU1(), v0 = sprite.getV0(), v1 = sprite.getV1();
            float du = u1-u0, dv = v1-v0;
            for (int i=0;i<4;i++) {
                int o=i*stride;
                p[i*3]   = Float.intBitsToFloat(packed[o])   + lx - chunk.key.x;
                p[i*3+1] = Float.intBitsToFloat(packed[o+1]) + ly - chunk.key.y;
                p[i*3+2] = Float.intBitsToFloat(packed[o+2]) + lz - chunk.key.z;
                float au = Float.intBitsToFloat(packed[o+4]);
                float av = Float.intBitsToFloat(packed[o+5]);
                uv[i*2]   = Math.abs(du) < 1e-7f ? 0f : (au-u0)/du;
                uv[i*2+1] = Math.abs(dv) < 1e-7f ? 0f : (av-v0)/dv;
            }
            chunk.render(layer).quads.add(new QuadDef(p,uv,texId));
            renderQuadCount++;
        }

        TextureDef loadTexture(ResourceLocation spriteId, TextureAtlasSprite sprite, String texId, int tint) {
            try {
                ResourceLocation png = ResourceLocation.fromNamespaceAndPath(spriteId.getNamespace(), "textures/" + spriteId.getPath() + ".png");
                var res = mc.getResourceManager().getResource(png);
                if (res.isPresent()) {
                    try (InputStream in = res.get().open()) {
                        BufferedImage source = ImageIO.read(in);
                        if (source != null) {
                            int fw = Math.min(source.getWidth(), sprite.contents().width());
                            int fh = Math.min(source.getHeight(), sprite.contents().height());
                            BufferedImage first = copyImage(source.getSubimage(0,0,fw,fh));
                            if (tint != 0xFFFFFF) applyTint(first, tint);
                            return new TextureDef(texId, first);
                        }
                    }
                }
            } catch (Throwable ignored) { }
            BufferedImage missing = new BufferedImage(16,16,BufferedImage.TYPE_INT_ARGB);
            for (int y=0;y<16;y++) for(int x=0;x<16;x++) missing.setRGB(x,y,(((x>>2)^(y>>2))&1)==0?0xffff00ff:0xff101010);
            return new TextureDef(texId, missing);
        }

        TextureDef loadStandaloneTexture(ResourceLocation spriteId, String texId, int tint) {
            try {
                ResourceLocation png = ResourceLocation.fromNamespaceAndPath(spriteId.getNamespace(), "textures/" + spriteId.getPath() + ".png");
                var res = mc.getResourceManager().getResource(png);
                if (res.isPresent()) {
                    try (InputStream in = res.get().open()) {
                        BufferedImage source = ImageIO.read(in);
                        if (source != null) {
                            // Animated fluid textures are vertical strips. Export the first square
                            // frame; CNR's baked map format is static and intentionally has no
                            // texture-animation runtime dependency.
                            int frame = Math.max(1, Math.min(source.getWidth(), source.getHeight()));
                            BufferedImage first = copyImage(source.getSubimage(0, 0, frame, frame));
                            if (tint != 0xFFFFFF) applyTint(first, tint);
                            return new TextureDef(texId, first);
                        }
                    }
                }
            } catch (Throwable ignored) { }
            BufferedImage missing = new BufferedImage(16,16,BufferedImage.TYPE_INT_ARGB);
            for (int y=0;y<16;y++) for(int x=0;x<16;x++) missing.setRGB(x,y,(((x>>2)^(y>>2))&1)==0?0xffff00ff:0xff101010);
            return new TextureDef(texId, missing);
        }

        void applyTint(BufferedImage image, int tint) {
            int tr=(tint>>16)&255, tg=(tint>>8)&255, tb=tint&255;
            for(int y=0;y<image.getHeight();y++) for(int x=0;x<image.getWidth();x++) {
                int argb=image.getRGB(x,y), a=(argb>>>24)&255;
                int r=(argb>>16)&255, g=(argb>>8)&255, b=argb&255;
                r=r*tr/255; g=g*tg/255; b=b*tb/255;
                image.setRGB(x,y,(a<<24)|(r<<16)|(g<<8)|b);
            }
        }

        void emitCollision(BlockState state, BlockPos pos, int lx, int ly, int lz, ChunkBuilder chunk, SnapshotView view,
                           CollisionBuilder target, boolean bulletPassThrough) {
            VoxelShape shape = state.getCollisionShape(view, pos);
            if (shape == null || shape.isEmpty()) return;
            for (AABB box : shape.toAabbs()) {
                target.addBox(
                    (float)box.minX + lx - chunk.key.x, (float)box.minY + ly - chunk.key.y, (float)box.minZ + lz - chunk.key.z,
                    (float)box.maxX + lx - chunk.key.x, (float)box.maxY + ly - chunk.key.y, (float)box.maxZ + lz - chunk.key.z);
                if (bulletPassThrough) bulletPassThroughBoxCount++;
                else collisionBoxCount++;
            }
        }

        void emitClimbable(BlockState state, BlockPos pos, int lx, int ly, int lz, ChunkBuilder chunk, SnapshotView view) {
            if (!state.is(BlockTags.CLIMBABLE)) return;

            // Interaction volume intentionally fills the whole Minecraft block cell.
            // Ladders/vines render as very thin planes against a solid backing block;
            // exporting that thin visual shape as a trigger leaves no room for a
            // CharacterController to overlap it before the wall stops the player.
            chunk.climbable.addBox(
                lx - chunk.key.x, ly - chunk.key.y, lz - chunk.key.z,
                lx - chunk.key.x + 1f, ly - chunk.key.y + 1f, lz - chunk.key.z + 1f);
            climbableBoxCount++;
        }

        String renderLayer(BlockState state) {
            RenderType type = ItemBlockRenderTypes.getChunkRenderType(state);
            if (type == RenderType.translucent()) return "transparent";
            if (type == RenderType.cutout() || type == RenderType.cutoutMipped()) return "cutout";
            return "opaque";
        }

        MapFile finish() throws Exception {
            MapFile out = new MapFile();
            out.format="cnr-dlc-map"; out.version=3; out.id=name; out.name=name; out.source="minecraft-fabric-1.21.1;compact-v3-q10-lz4";
            out.blockScale=1f; out.origin=new float[]{0,0,0};
            out.atlas = atlas.toJson();
            out.chunks = new ArrayList<>();
            for (ChunkBuilder c : chunks.values()) out.chunks.add(c.toJson(atlas));
            out.spawns = new ArrayList<>();
            out.copSpawns = new ArrayList<>(copSpawns);
            out.robberSpawns = new ArrayList<>(robberSpawns);
            return out;
        }
    }

    private static BufferedImage copyImage(BufferedImage src) {
        BufferedImage out = new BufferedImage(src.getWidth(),src.getHeight(),BufferedImage.TYPE_INT_ARGB);
        Graphics2D g=out.createGraphics(); g.drawImage(src,0,0,null); g.dispose(); return out;
    }

    private record ChunkKey(int x,int y,int z) { }
    private record QuadDef(float[] p,float[] uv,String texture) { }

    private static final class TextureDef {
        final String id; final BufferedImage image; AtlasRect rect;
        TextureDef(String id, BufferedImage image) { this.id=id; this.image=image; }
    }
    private record AtlasRect(int x,int y,int w,int h) { }

    private static final class Atlas {
        final BufferedImage image; final Map<String,AtlasRect> rects;
        Atlas(BufferedImage image, Map<String,AtlasRect> rects) { this.image=image; this.rects=rects; }

        static Atlas pack(Collection<TextureDef> defs) {
            List<TextureDef> list=new ArrayList<>(defs);
            list.sort(Comparator.comparingInt((TextureDef d)->d.image.getHeight()).reversed());
            for (int size : new int[]{256,512,1024,2048,4096,8192}) {
                Map<String,AtlasRect> rects=new LinkedHashMap<>(); int x=1,y=1,rowH=0; boolean ok=true;
                for(TextureDef d:list){ int w=d.image.getWidth()+2,h=d.image.getHeight()+2; if(w>size||h>size){ok=false;break;} if(x+w>size){x=1;y+=rowH;rowH=0;} if(y+h>size){ok=false;break;} AtlasRect r=new AtlasRect(x,y,d.image.getWidth(),d.image.getHeight()); d.rect=r;rects.put(d.id,r);x+=w;rowH=Math.max(rowH,h); }
                if(!ok) continue;
                BufferedImage atlas=new BufferedImage(size,size,BufferedImage.TYPE_INT_ARGB); Graphics2D g=atlas.createGraphics();
                for(TextureDef d:list){ AtlasRect r=d.rect; g.drawImage(d.image,r.x,r.y,null); // one-pixel gutters
                    g.drawImage(d.image,r.x,r.y-1,r.x+r.w,r.y,0,0,r.w,1,null); g.drawImage(d.image,r.x,r.y+r.h,r.x+r.w,r.y+r.h+1,0,r.h-1,r.w,r.h,null);
                    g.drawImage(d.image,r.x-1,r.y,r.x,r.y+r.h,0,0,1,r.h,null); g.drawImage(d.image,r.x+r.w,r.y,r.x+r.w+1,r.y+r.h,r.w-1,0,r.w,r.h,null);
                }
                g.dispose(); return new Atlas(atlas,rects);
            }
            throw new IllegalStateException("Texture atlas exceeds 8192x8192");
        }

        AtlasJson toJson() throws Exception {
            ByteArrayOutputStream bytes=new ByteArrayOutputStream(); ImageIO.write(image,"png",bytes);
            AtlasJson j=new AtlasJson(); j.width=image.getWidth(); j.height=image.getHeight(); j.pngBase64=base64ForJson(bytes.toByteArray());
            j.entries=new ArrayList<>(); for(var e:rects.entrySet()){ AtlasEntry a=new AtlasEntry(); a.id=e.getKey(); AtlasRect r=e.getValue();a.x=r.x;a.y=r.y;a.w=r.w;a.h=r.h;j.entries.add(a);} return j;
        }
    }

    private static final class RenderBuilder { final List<QuadDef> quads=new ArrayList<>(); }
    private static final class CollisionBuilder {
        final List<float[]> boxes=new ArrayList<>();
        void addBox(float x0,float y0,float z0,float x1,float y1,float z1){ if(x1>x0&&y1>y0&&z1>z0) boxes.add(new float[]{x0,y0,z0,x1,y1,z1}); }
    }
    private static final class ChunkBuilder {
        final ChunkKey key; final RenderBuilder opaque=new RenderBuilder(),cutout=new RenderBuilder(),transparent=new RenderBuilder(); final CollisionBuilder collision=new CollisionBuilder(),bulletPassThrough=new CollisionBuilder(),climbable=new CollisionBuilder(),water=new CollisionBuilder();
        ChunkBuilder(ChunkKey key){this.key=key;}
        RenderBuilder render(String s){return s.equals("transparent")?transparent:s.equals("cutout")?cutout:opaque;}
        ChunkJson toJson(Atlas atlas){ ChunkJson j=new ChunkJson();j.x=key.x;j.y=key.y;j.z=key.z;j.opaquePacked=packRender(opaque,atlas);j.cutoutPacked=packRender(cutout,atlas);j.transparentPacked=packRender(transparent,atlas);j.collisionBoxesPacked=packBoxes(collision);j.bulletPassThroughBoxesPacked=packBoxes(bulletPassThrough);j.climbableBoxesPacked=packBoxes(climbable);j.waterBoxesPacked=packBoxes(water);return j; }
    }

    private static List<MeshJson> meshRender(RenderBuilder src, Atlas atlas) {
        List<MeshJson> out=new ArrayList<>(); MeshAccumulator m=new MeshAccumulator(true);
        for(QuadDef q:src.quads){ if(m.vertexCount()+4>MAX_VERTICES_PER_PART){out.add(m.finish());m=new MeshAccumulator(true);} AtlasRect r=atlas.rects.get(q.texture); if(r==null) continue; float[] auv=new float[8]; for(int i=0;i<4;i++){ auv[i*2]=(r.x+q.uv[i*2]*r.w)/(float)atlas.image.getWidth(); auv[i*2+1]=1f-(r.y+q.uv[i*2+1]*r.h)/(float)atlas.image.getHeight(); } m.addQuad(q.p,auv); }
        if(m.vertexCount()>0)out.add(m.finish()); return out;
    }

    private static List<PackedBlob> packRender(RenderBuilder src, Atlas atlas) {
        List<PackedBlob> out=new ArrayList<>();
        for(MeshJson m:meshRender(src,atlas)) out.add(packMesh(m));
        return out;
    }

    private static PackedBlob packMesh(MeshJson m) {
        int vc=m.vertices.length/3;
        if(vc<=0 || vc>65535 || (vc&3)!=0) throw new IllegalStateException("Packed mesh vertex count must be a non-empty multiple of four and <= 65535");
        if(m.uv.length!=vc*2) throw new IllegalStateException("Packed mesh UV count does not match its vertices");
        if(!isImplicitQuadTriangles(m.triangles,vc)) throw new IllegalStateException("Packed mesh is not in exporter quad order");

        int bytes=4 + m.vertices.length*2 + m.uv.length*2;
        ByteBuffer b=ByteBuffer.allocate(bytes).order(ByteOrder.LITTLE_ENDIAN);
        b.putInt(vc);
        for(float f:m.vertices)b.putShort(packPosition(f));
        for(float f:m.uv)b.putShort(packUv(f));
        return packBinary(b.array(), "cnrmesh-q10-u16-quads-raw-v1", "cnrmesh-q10-u16-quads-lz4-v1", 0);
    }

    private static PackedBlob packBoxes(CollisionBuilder src) {
        List<float[]> boxes=mergeCollisionBoxes(src.boxes);
        ByteBuffer b=ByteBuffer.allocate(4+boxes.size()*12).order(ByteOrder.LITTLE_ENDIAN);
        b.putInt(boxes.size());
        for(float[] box:boxes)for(float f:box)b.putShort(packPosition(f));
        return packBinary(b.array(), "cnrboxes-q10-raw-v1", "cnrboxes-q10-lz4-v1", boxes.size());
    }

    private static short packPosition(float value) {
        int q=Math.round(value*PACKED_POSITION_SCALE);
        if(q<Short.MIN_VALUE || q>Short.MAX_VALUE) throw new IllegalStateException("Packed coordinate is outside q10 range: "+value);
        return (short)q;
    }

    private static short packUv(float value) {
        if(value < -0.0001f || value > 1.0001f) throw new IllegalStateException("Packed UV is outside normalized range: "+value);
        float clamped=Math.max(0f,Math.min(1f,value));
        return (short)(Math.round(clamped*PACKED_UV_SCALE) & 0xffff);
    }

    private static boolean isImplicitQuadTriangles(int[] triangles,int vertexCount) {
        if(triangles==null || triangles.length!=(vertexCount/4)*6) return false;
        int ti=0;
        for(int v=0;v<vertexCount;v+=4) {
            if(triangles[ti++]!=v || triangles[ti++]!=v+1 || triangles[ti++]!=v+2 || triangles[ti++]!=v || triangles[ti++]!=v+2 || triangles[ti++]!=v+3) return false;
        }
        return true;
    }

    private static PackedBlob packBinary(byte[] raw, String rawEncoding, String lz4Encoding, int count) {
        byte[] compressed=lz4Compress(raw);
        PackedBlob p=new PackedBlob();
        p.count=count;
        p.rawBytes=raw.length;
        // Base64 grows either form by the same ratio, so compare the binary payloads.
        // Keep tiny or incompressible parts raw instead of making them larger.
        if(compressed.length + 4 < raw.length){
            p.encoding=lz4Encoding;
            p.dataBase64=base64ForJson(compressed);
        }else{
            p.encoding=rawEncoding;
            p.dataBase64=base64ForJson(raw);
        }
        return p;
    }

    private static byte[] lz4Compress(byte[] src) {
        if(src.length==0) return new byte[]{0};
        int[] table=new int[1<<16];
        Arrays.fill(table,-1);
        ByteArrayOutputStream out=new ByteArrayOutputStream(src.length);
        int anchor=0;
        int i=0;
        int limit=src.length-4;
        while(i<=limit){
            int h=lz4Hash(src,i);
            int ref=table[h];
            table[h]=i;
            if(ref<0 || i-ref>65535 || !lz4Equal4(src,ref,i)){
                i++;
                continue;
            }

            int matchLength=4;
            while(i+matchLength<src.length && src[ref+matchLength]==src[i+matchLength]) matchLength++;
            int literalLength=i-anchor;
            int encodedMatchLength=matchLength-4;
            int token=(Math.min(literalLength,15)<<4) | Math.min(encodedMatchLength,15);
            out.write(token);
            if(literalLength>=15) writeLz4Length(out,literalLength-15);
            out.write(src,anchor,literalLength);

            int offset=i-ref;
            out.write(offset & 255);
            out.write((offset>>>8) & 255);
            if(encodedMatchLength>=15) writeLz4Length(out,encodedMatchLength-15);

            int end=i+matchLength;
            for(int p=i+1;p<end && p<=limit;p++) table[lz4Hash(src,p)]=p;
            i=end;
            anchor=i;
        }

        int literalLength=src.length-anchor;
        out.write(Math.min(literalLength,15)<<4);
        if(literalLength>=15) writeLz4Length(out,literalLength-15);
        out.write(src,anchor,literalLength);
        return out.toByteArray();
    }

    private static int lz4Hash(byte[] src,int p) {
        int v=(src[p]&255) | ((src[p+1]&255)<<8) | ((src[p+2]&255)<<16) | ((src[p+3]&255)<<24);
        return (v * -1640531535) >>> 16;
    }

    private static boolean lz4Equal4(byte[] src,int a,int b) {
        return src[a]==src[b] && src[a+1]==src[b+1] && src[a+2]==src[b+2] && src[a+3]==src[b+3];
    }

    private static void writeLz4Length(ByteArrayOutputStream out,int length) {
        while(length>=255){ out.write(255); length-=255; }
        out.write(length);
    }

    private static String base64ForJson(byte[] data) {
        String encoded=Base64.getEncoder().encodeToString(data);
        if(encoded.length()<=60) return encoded;
        StringBuilder out=new StringBuilder(encoded.length()+encoded.length()/60+1);
        for(int i=0;i<encoded.length();i+=60) {
            if(i>0) out.append('\n');
            out.append(encoded,i,Math.min(encoded.length(),i+60));
        }
        return out.toString();
    }

    private static int q(float v){return Math.round(v*4096f);}
    private static List<float[]> mergeCollisionBoxes(List<float[]> input) {
        List<float[]> boxes=new ArrayList<>(); for(float[] b:input)boxes.add(Arrays.copyOf(b,6));
        int previous=-1;
        while(previous!=boxes.size()){
            previous=boxes.size();
            boxes=mergeAxis(boxes,0); boxes=mergeAxis(boxes,1); boxes=mergeAxis(boxes,2);
        }
        return boxes;
    }
    private static List<float[]> mergeAxis(List<float[]> boxes,int axis) {
        int a=(axis+1)%3,b=(axis+2)%3;
        Map<String,List<float[]>> groups=new LinkedHashMap<>();
        for(float[] box:boxes){String k=q(box[a])+":"+q(box[a+3])+":"+q(box[b])+":"+q(box[b+3]);groups.computeIfAbsent(k,x->new ArrayList<>()).add(box);}
        List<float[]> out=new ArrayList<>();
        for(List<float[]> arr:groups.values()){
            arr.sort(Comparator.comparingInt(x->q(x[axis])));
            float[] cur=Arrays.copyOf(arr.get(0),6);
            for(int i=1;i<arr.size();i++){float[] n=arr.get(i);if(q(cur[axis+3])==q(n[axis]))cur[axis+3]=n[axis+3];else{out.add(cur);cur=Arrays.copyOf(n,6);}}
            out.add(cur);
        }
        return out;
    }

    private static final class MeshAccumulator {
        final boolean uvEnabled; final List<Float> v=new ArrayList<>(),uv=new ArrayList<>(); final List<Integer> t=new ArrayList<>();
        MeshAccumulator(boolean uvEnabled){this.uvEnabled=uvEnabled;} int vertexCount(){return v.size()/3;}
        void addQuad(float[] p,float[] u){int b=vertexCount();for(float f:p)v.add(f);if(uvEnabled)for(float f:u)uv.add(f); t.add(b);t.add(b+1);t.add(b+2);t.add(b);t.add(b+2);t.add(b+3);}
        MeshJson finish(){MeshJson j=new MeshJson();j.vertices=new float[v.size()];for(int i=0;i<v.size();i++)j.vertices[i]=v.get(i);j.uv=uvEnabled?new float[uv.size()]:new float[0];if(uvEnabled)for(int i=0;i<uv.size();i++)j.uv[i]=uv.get(i);j.triangles=new int[t.size()];for(int i=0;i<t.size();i++)j.triangles[i]=t.get(i);return j;}
    }

    private static final class MapFile { String format,id,name,source; int version; float blockScale; float[] origin; AtlasJson atlas; List<ChunkJson> chunks; List<float[]> spawns,copSpawns,robberSpawns; }
    private static final class AtlasJson { int width,height; String pngBase64; List<AtlasEntry> entries; }
    private static final class AtlasEntry { String id; int x,y,w,h; }
    private static final class ChunkJson { int x,y,z; List<PackedBlob> opaquePacked,cutoutPacked,transparentPacked; PackedBlob collisionBoxesPacked,bulletPassThroughBoxesPacked,climbableBoxesPacked,waterBoxesPacked; }
    private static final class PackedBlob { String encoding,dataBase64; int count,rawBytes; }
    private static final class MeshJson { float[] vertices,uv; int[] triangles; }
}
