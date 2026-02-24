.class public interface abstract Lcom/kamcord/android/AudioSource;
.super Ljava/lang/Object;


# virtual methods
.method public abstract getNumAudioSamplesReady()I
.end method

.method public abstract getNumBytesPerChannel()I
.end method

.method public abstract getNumChannels()I
.end method

.method public abstract getSampleRate()I
.end method

.method public abstract obtainAudioSamples(Ljava/nio/ByteBuffer;I)V
.end method

.method public abstract skip()V
.end method

.method public abstract start()V
.end method

.method public abstract stop()V
.end method
