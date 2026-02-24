.class public Lcom/kamcord/android/FmodAudioSource;
.super Ljava/lang/Object;

# interfaces
.implements Lcom/kamcord/android/AudioSource;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public getNumAudioSamplesReady()I
    .locals 1

    invoke-static {}, Lcom/kamcord/android/core/KamcordNative;->getNumAudioBytesReady()I

    move-result v0

    div-int/lit8 v0, v0, 0x4

    return v0
.end method

.method public getNumBytesPerChannel()I
    .locals 1

    const/4 v0, 0x2

    return v0
.end method

.method public getNumChannels()I
    .locals 1

    const/4 v0, 0x2

    return v0
.end method

.method public getSampleRate()I
    .locals 1

    const/16 v0, 0x5dc0

    return v0
.end method

.method public obtainAudioSamples(Ljava/nio/ByteBuffer;I)V
    .locals 1

    shl-int/lit8 v0, p2, 0x2

    invoke-static {p1, v0}, Lcom/kamcord/android/core/KamcordNative;->obtainAudioBytes(Ljava/nio/ByteBuffer;I)V

    return-void
.end method

.method public skip()V
    .locals 0

    invoke-static {}, Lcom/kamcord/android/core/KamcordNative;->skipAudioBytes()V

    return-void
.end method

.method public start()V
    .locals 1

    const/4 v0, 0x1

    invoke-static {v0}, Lcom/kamcord/android/core/KamcordNative;->setCircularBufferEnabled(Z)V

    return-void
.end method

.method public stop()V
    .locals 1

    const/4 v0, 0x0

    invoke-static {v0}, Lcom/kamcord/android/core/KamcordNative;->setCircularBufferEnabled(Z)V

    return-void
.end method
