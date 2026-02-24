.class abstract Lcom/kamcord/android/core/KC_M;
.super Lcom/kamcord/android/core/KC_I;

# interfaces
.implements Lcom/kamcord/android/core/KC_j;


# instance fields
.field protected b:Lcom/kamcord/android/core/KC_x;

.field protected c:Lcom/kamcord/android/core/KC_y;

.field private d:Lcom/kamcord/android/core/KC_i;

.field private e:Landroid/media/MediaCodec;

.field private f:Landroid/opengl/EGLDisplay;

.field private g:Landroid/opengl/EGLSurface;

.field private h:Landroid/opengl/EGLSurface;

.field private i:Landroid/opengl/EGLContext;

.field private j:Lcom/kamcord/android/core/KC_V;


# direct methods
.method constructor <init>(Lcom/kamcord/android/core/KC_x;Lcom/kamcord/android/core/KC_V;)V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_I;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/core/KC_M;->b:Lcom/kamcord/android/core/KC_x;

    iput-object p2, p0, Lcom/kamcord/android/core/KC_M;->j:Lcom/kamcord/android/core/KC_V;

    return-void
.end method

.method private o()Z
    .locals 4

    const/4 v0, 0x1

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/android/core/KC_M;->j:Lcom/kamcord/android/core/KC_V;

    monitor-enter v1
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    :try_start_1
    iget-object v2, p0, Lcom/kamcord/android/core/KC_M;->j:Lcom/kamcord/android/core/KC_V;

    const/4 v3, 0x1

    iput-boolean v3, v2, Lcom/kamcord/android/core/KC_V;->a:Z

    iget-object v2, p0, Lcom/kamcord/android/core/KC_M;->j:Lcom/kamcord/android/core/KC_V;

    invoke-virtual {v2}, Ljava/lang/Object;->wait()V

    iget-object v2, p0, Lcom/kamcord/android/core/KC_M;->d:Lcom/kamcord/android/core/KC_i;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_i;->c()V

    monitor-exit v1
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :goto_0
    return v0

    :catchall_0
    move-exception v0

    :try_start_2
    monitor-exit v1

    throw v0
    :try_end_2
    .catch Ljava/lang/Exception; {:try_start_2 .. :try_end_2} :catch_0

    :catch_0
    move-exception v0

    const/4 v0, 0x0

    goto :goto_0
.end method


# virtual methods
.method public final a()V
    .locals 0

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_M;->n()Z

    return-void
.end method

.method public final a(Lcom/kamcord/android/core/KC_h;Lcom/kamcord/android/core/KC_f;)Z
    .locals 4

    const/4 v0, 0x0

    iget-object v1, p1, Lcom/kamcord/android/core/KC_h;->a:Landroid/media/MediaCodec;

    iput-object v1, p0, Lcom/kamcord/android/core/KC_M;->e:Landroid/media/MediaCodec;

    const-string v1, "Initializing codec input.  Saving render state."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_M;->l()V

    const-string v1, "...done saving render state."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v1, p0, Lcom/kamcord/android/core/KC_M;->d:Lcom/kamcord/android/core/KC_i;

    if-eqz v1, :cond_0

    const-string v1, "Input surface already exists, initialized twice?"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :cond_0
    :try_start_0
    const-string v1, "Creating video codec input surface."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v1, Lcom/kamcord/android/core/KC_i;

    iget-object v2, p0, Lcom/kamcord/android/core/KC_M;->e:Landroid/media/MediaCodec;

    invoke-virtual {v2}, Landroid/media/MediaCodec;->createInputSurface()Landroid/view/Surface;

    move-result-object v2

    invoke-direct {v1, v2}, Lcom/kamcord/android/core/KC_i;-><init>(Landroid/view/Surface;)V

    iput-object v1, p0, Lcom/kamcord/android/core/KC_M;->d:Lcom/kamcord/android/core/KC_i;

    const-string v1, "...done creating video codec input surface."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    const-string v1, "Restoring render state."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_M;->m()V

    const-string v1, "...done restring render state."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v1, Lcom/kamcord/android/core/KC_y;

    invoke-direct {v1}, Lcom/kamcord/android/core/KC_y;-><init>()V

    iput-object v1, p0, Lcom/kamcord/android/core/KC_M;->c:Lcom/kamcord/android/core/KC_y;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_M;->c:Lcom/kamcord/android/core/KC_y;

    iget-object v2, p1, Lcom/kamcord/android/core/KC_h;->c:Lcom/kamcord/android/core/KC_f;

    iget v2, v2, Lcom/kamcord/android/core/KC_f;->a:I

    iput v2, v1, Lcom/kamcord/android/core/KC_y;->e:I

    iget-object v1, p0, Lcom/kamcord/android/core/KC_M;->c:Lcom/kamcord/android/core/KC_y;

    iget-object v2, p1, Lcom/kamcord/android/core/KC_h;->c:Lcom/kamcord/android/core/KC_f;

    iget v2, v2, Lcom/kamcord/android/core/KC_f;->b:I

    iput v2, v1, Lcom/kamcord/android/core/KC_y;->f:I

    iget-object v1, p0, Lcom/kamcord/android/core/KC_M;->c:Lcom/kamcord/android/core/KC_y;

    iput v0, v1, Lcom/kamcord/android/core/KC_y;->c:I

    iget-object v1, p0, Lcom/kamcord/android/core/KC_M;->c:Lcom/kamcord/android/core/KC_y;

    iput v0, v1, Lcom/kamcord/android/core/KC_y;->d:I

    const/4 v0, 0x1

    :goto_0
    return v0

    :catch_0
    move-exception v1

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Codec-input initialization exception: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_M;->f()V

    goto :goto_0
.end method

.method protected final b(Lcom/kamcord/android/core/KC_F;)J
    .locals 4

    iget-wide v0, p1, Lcom/kamcord/android/core/KC_F;->b:J

    invoke-virtual {p0, v0, v1}, Lcom/kamcord/android/core/KC_M;->a(J)J

    move-result-wide v0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_M;->e:Landroid/media/MediaCodec;

    monitor-enter v2

    :try_start_0
    iget-object v3, p0, Lcom/kamcord/android/core/KC_M;->d:Lcom/kamcord/android/core/KC_i;

    invoke-virtual {v3, v0, v1}, Lcom/kamcord/android/core/KC_i;->a(J)V

    iget-object v3, p0, Lcom/kamcord/android/core/KC_M;->d:Lcom/kamcord/android/core/KC_i;

    invoke-virtual {v3}, Lcom/kamcord/android/core/KC_i;->d()Z

    monitor-exit v2
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    return-wide v0

    :catchall_0
    move-exception v0

    monitor-exit v2

    throw v0
.end method

.method public final b()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_M;->o()Z

    return-void
.end method

.method public final c()[I
    .locals 3

    const/4 v0, 0x1

    new-array v0, v0, [I

    const/4 v1, 0x0

    const v2, 0x7f000789

    aput v2, v0, v1

    return-object v0
.end method

.method public final f()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_M;->d:Lcom/kamcord/android/core/KC_i;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_M;->d:Lcom/kamcord/android/core/KC_i;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_i;->a()V

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_M;->d:Lcom/kamcord/android/core/KC_i;

    :cond_0
    return-void
.end method

.method public final g()Z
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/core/KC_M;->d:Lcom/kamcord/android/core/KC_i;

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method protected final l()V
    .locals 1

    invoke-static {}, Landroid/opengl/EGL14;->eglGetCurrentDisplay()Landroid/opengl/EGLDisplay;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_M;->f:Landroid/opengl/EGLDisplay;

    const/16 v0, 0x3059

    invoke-static {v0}, Landroid/opengl/EGL14;->eglGetCurrentSurface(I)Landroid/opengl/EGLSurface;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_M;->g:Landroid/opengl/EGLSurface;

    const/16 v0, 0x305a

    invoke-static {v0}, Landroid/opengl/EGL14;->eglGetCurrentSurface(I)Landroid/opengl/EGLSurface;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_M;->h:Landroid/opengl/EGLSurface;

    invoke-static {}, Landroid/opengl/EGL14;->eglGetCurrentContext()Landroid/opengl/EGLContext;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_M;->i:Landroid/opengl/EGLContext;

    return-void
.end method

.method protected final m()V
    .locals 4

    iget-object v0, p0, Lcom/kamcord/android/core/KC_M;->f:Landroid/opengl/EGLDisplay;

    sget-object v1, Landroid/opengl/EGL14;->EGL_NO_DISPLAY:Landroid/opengl/EGLDisplay;

    if-ne v0, v1, :cond_1

    :cond_0
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_M;->f:Landroid/opengl/EGLDisplay;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_M;->g:Landroid/opengl/EGLSurface;

    iget-object v2, p0, Lcom/kamcord/android/core/KC_M;->h:Landroid/opengl/EGLSurface;

    iget-object v3, p0, Lcom/kamcord/android/core/KC_M;->i:Landroid/opengl/EGLContext;

    invoke-static {v0, v1, v2, v3}, Landroid/opengl/EGL14;->eglMakeCurrent(Landroid/opengl/EGLDisplay;Landroid/opengl/EGLSurface;Landroid/opengl/EGLSurface;Landroid/opengl/EGLContext;)Z

    move-result v0

    if-nez v0, :cond_0

    new-instance v0, Ljava/lang/RuntimeException;

    const-string v1, "eglMakeCurrent failed"

    invoke-direct {v0, v1}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/String;)V

    throw v0
.end method

.method protected final n()Z
    .locals 4

    const/4 v0, 0x1

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/android/core/KC_M;->j:Lcom/kamcord/android/core/KC_V;

    monitor-enter v1
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    :try_start_1
    iget-object v2, p0, Lcom/kamcord/android/core/KC_M;->j:Lcom/kamcord/android/core/KC_V;

    const/4 v3, 0x1

    iput-boolean v3, v2, Lcom/kamcord/android/core/KC_V;->a:Z

    iget-object v2, p0, Lcom/kamcord/android/core/KC_M;->j:Lcom/kamcord/android/core/KC_V;

    invoke-virtual {v2}, Ljava/lang/Object;->wait()V

    iget-object v2, p0, Lcom/kamcord/android/core/KC_M;->d:Lcom/kamcord/android/core/KC_i;

    invoke-virtual {v2}, Lcom/kamcord/android/core/KC_i;->b()V

    monitor-exit v1
    :try_end_1
    .catchall {:try_start_1 .. :try_end_1} :catchall_0

    :goto_0
    return v0

    :catchall_0
    move-exception v0

    :try_start_2
    monitor-exit v1

    throw v0
    :try_end_2
    .catch Ljava/lang/Exception; {:try_start_2 .. :try_end_2} :catch_0

    :catch_0
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Exception;->printStackTrace()V

    const/4 v0, 0x0

    goto :goto_0
.end method
