.class public final Lcom/kamcord/android/KC_v;
.super Ljava/lang/Object;

# interfaces
.implements Lcom/kamcord/android/AudioSource;


# instance fields
.field private a:I

.field private b:I


# direct methods
.method public constructor <init>(II)V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput p1, p0, Lcom/kamcord/android/KC_v;->a:I

    iput p2, p0, Lcom/kamcord/android/KC_v;->b:I

    invoke-static {p2, p2}, Lcom/kamcord/android/core/KamcordNative;->setNumChannels(II)V

    return-void
.end method

.method public static a([FI)V
    .locals 0

    invoke-static {p0, p1}, Lcom/kamcord/android/core/KamcordNative;->writeAudioData([FI)V

    return-void
.end method


# virtual methods
.method public final getNumAudioSamplesReady()I
    .locals 1

    invoke-static {}, Lcom/kamcord/android/core/KamcordNative;->getNumAudioBytesFromCircularBuffer()I

    move-result v0

    div-int/lit8 v0, v0, 0x4

    return v0
.end method

.method public final getNumBytesPerChannel()I
    .locals 1

    const/4 v0, 0x2

    return v0
.end method

.method public final getNumChannels()I
    .locals 1

    iget v0, p0, Lcom/kamcord/android/KC_v;->b:I

    return v0
.end method

.method public final getSampleRate()I
    .locals 1

    iget v0, p0, Lcom/kamcord/android/KC_v;->a:I

    return v0
.end method

.method public final obtainAudioSamples(Ljava/nio/ByteBuffer;I)V
    .locals 1

    shl-int/lit8 v0, p2, 0x2

    invoke-static {p1, v0}, Lcom/kamcord/android/core/KamcordNative;->obtainAudioBytes(Ljava/nio/ByteBuffer;I)V

    return-void
.end method

.method public final skip()V
    .locals 0

    return-void
.end method

.method public final start()V
    .locals 1

    const/4 v0, 0x1

    invoke-static {v0}, Lcom/kamcord/android/core/KamcordNative;->setCircularBufferEnabled(Z)V

    return-void
.end method

.method public final stop()V
    .locals 1

    const/4 v0, 0x0

    invoke-static {v0}, Lcom/kamcord/android/core/KamcordNative;->setCircularBufferEnabled(Z)V

    return-void
.end method
