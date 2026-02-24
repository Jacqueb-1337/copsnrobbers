.class public Lcom/kamcord/android/core/KamcordNative;
.super Ljava/lang/Object;


# direct methods
.method static native computeGraphicBufferStride(IIIZ)V
.end method

.method static native copyTextureToBuffer(ILjava/nio/ByteBuffer;IIIZZIIIIII)V
.end method

.method static native deleteEGLImageTexture(I)V
.end method

.method public static native getNumAudioBytesFromCircularBuffer()I
.end method

.method public static native getNumAudioBytesReady()I
.end method

.method public static native glBindFramebuffer(II)V
.end method

.method public static native glGetIntegerv(I[II)V
.end method

.method public static native initDefaultFramebuffer()V
.end method

.method static native newEGLImageTexture(IIZ)I
.end method

.method public static native obtainAudioBytes(Ljava/nio/ByteBuffer;I)V
.end method

.method public static native setCircularBufferEnabled(Z)V
.end method

.method public static native setCurrentThreadAffinity(I)V
.end method

.method public static native setDefaultFramebuffer(I)V
.end method

.method public static native setNumChannels(II)V
.end method

.method public static native skipAudioBytes()V
.end method

.method public static native writeAudioData([FI)V
.end method
