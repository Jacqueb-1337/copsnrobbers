.class Lcom/kamcord/android/core/KC_y;
.super Ljava/lang/Object;


# instance fields
.field a:I

.field b:I

.field c:I

.field d:I

.field e:I

.field f:I

.field volatile g:Z

.field private h:I

.field private i:[I


# direct methods
.method constructor <init>()V
    .locals 1

    const/4 v0, 0x1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_y;->g:Z

    new-array v0, v0, [I

    iput-object v0, p0, Lcom/kamcord/android/core/KC_y;->i:[I

    return-void
.end method


# virtual methods
.method protected a(IILcom/kamcord/android/core/KC_p;)I
    .locals 15

    const/4 v1, 0x1

    iget-object v2, p0, Lcom/kamcord/android/core/KC_y;->i:[I

    const/4 v3, 0x0

    invoke-static {v1, v2, v3}, Landroid/opengl/GLES20;->glGenTextures(I[II)V

    iget-object v1, p0, Lcom/kamcord/android/core/KC_y;->i:[I

    const/4 v2, 0x0

    aget v10, v1, v2

    const v1, 0x84c0

    invoke-static {v1}, Landroid/opengl/GLES20;->glActiveTexture(I)V

    const/16 v1, 0xde1

    invoke-static {v1, v10}, Landroid/opengl/GLES20;->glBindTexture(II)V

    const/4 v1, 0x1

    new-array v11, v1, [I

    const/4 v1, 0x1

    new-array v12, v1, [I

    const/4 v1, 0x1

    new-array v13, v1, [I

    const/4 v1, 0x1

    new-array v14, v1, [I

    const/16 v1, 0xde1

    const/16 v2, 0x2801

    const/4 v3, 0x0

    invoke-static {v1, v2, v11, v3}, Landroid/opengl/GLES20;->glGetTexParameteriv(II[II)V

    const/4 v1, 0x0

    aget v1, v11, v1

    const/16 v2, 0x2601

    if-eq v1, v2, :cond_0

    const/16 v1, 0xde1

    const/16 v2, 0x2801

    const/16 v3, 0x2601

    invoke-static {v1, v2, v3}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_0
    const/16 v1, 0xde1

    const/16 v2, 0x2800

    const/4 v3, 0x0

    invoke-static {v1, v2, v12, v3}, Landroid/opengl/GLES20;->glGetTexParameteriv(II[II)V

    const/4 v1, 0x0

    aget v1, v12, v1

    const/16 v2, 0x2601

    if-eq v1, v2, :cond_1

    const/16 v1, 0xde1

    const/16 v2, 0x2800

    const/16 v3, 0x2601

    invoke-static {v1, v2, v3}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_1
    const/16 v1, 0xde1

    const/16 v2, 0x2802

    const/4 v3, 0x0

    invoke-static {v1, v2, v13, v3}, Landroid/opengl/GLES20;->glGetTexParameteriv(II[II)V

    const/4 v1, 0x0

    aget v1, v13, v1

    const v2, 0x812f

    if-eq v1, v2, :cond_2

    const/16 v1, 0xde1

    const/16 v2, 0x2802

    const v3, 0x812f

    invoke-static {v1, v2, v3}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_2
    const/16 v1, 0xde1

    const/16 v2, 0x2803

    const/4 v3, 0x0

    invoke-static {v1, v2, v14, v3}, Landroid/opengl/GLES20;->glGetTexParameteriv(II[II)V

    const/4 v1, 0x0

    aget v1, v14, v1

    const v2, 0x812f

    if-eq v1, v2, :cond_3

    const/16 v1, 0xde1

    const/16 v2, 0x2803

    const v3, 0x812f

    invoke-static {v1, v2, v3}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_3
    move-object/from16 v0, p3

    iget v3, v0, Lcom/kamcord/android/core/KC_p;->b:I

    const/16 v1, 0x1907

    if-ne v3, v1, :cond_8

    const/16 v7, 0x1907

    const v8, 0x8363

    :goto_0
    const/16 v1, 0xde1

    const/4 v2, 0x0

    const/4 v6, 0x0

    const/4 v9, 0x0

    move/from16 v4, p1

    move/from16 v5, p2

    invoke-static/range {v1 .. v9}, Landroid/opengl/GLES20;->glTexImage2D(IIIIIIIILjava/nio/Buffer;)V

    const/16 v1, 0xde1

    const/4 v2, 0x0

    invoke-static {v1, v2}, Landroid/opengl/GLES20;->glBindTexture(II)V

    const/4 v1, 0x0

    aget v1, v14, v1

    const v2, 0x812f

    if-eq v1, v2, :cond_4

    const/16 v1, 0xde1

    const/16 v2, 0x2803

    const/4 v3, 0x0

    aget v3, v14, v3

    invoke-static {v1, v2, v3}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_4
    const/4 v1, 0x0

    aget v1, v13, v1

    const v2, 0x812f

    if-eq v1, v2, :cond_5

    const/16 v1, 0xde1

    const/16 v2, 0x2802

    const/4 v3, 0x0

    aget v3, v13, v3

    invoke-static {v1, v2, v3}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_5
    const/4 v1, 0x0

    aget v1, v12, v1

    const/16 v2, 0x2601

    if-eq v1, v2, :cond_6

    const/16 v1, 0xde1

    const/16 v2, 0x2800

    const/4 v3, 0x0

    aget v3, v12, v3

    invoke-static {v1, v2, v3}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_6
    const/4 v1, 0x0

    aget v1, v11, v1

    const/16 v2, 0x2601

    if-eq v1, v2, :cond_7

    const/16 v1, 0xde1

    const/16 v2, 0x2801

    const/4 v3, 0x0

    aget v3, v11, v3

    invoke-static {v1, v2, v3}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_7
    return v10

    :cond_8
    const/16 v7, 0x1908

    const/16 v8, 0x1401

    goto :goto_0
.end method

.method final a()V
    .locals 4

    const/4 v3, 0x1

    const/4 v2, 0x0

    const v0, 0x84c0

    invoke-static {v0}, Landroid/opengl/GLES20;->glActiveTexture(I)V

    iget v0, p0, Lcom/kamcord/android/core/KC_y;->a:I

    if-eqz v0, :cond_0

    new-array v0, v3, [I

    iget v1, p0, Lcom/kamcord/android/core/KC_y;->a:I

    aput v1, v0, v2

    invoke-static {v3, v0, v2}, Landroid/opengl/GLES20;->glDeleteFramebuffers(I[II)V

    iput v2, p0, Lcom/kamcord/android/core/KC_y;->a:I

    :cond_0
    iget v0, p0, Lcom/kamcord/android/core/KC_y;->h:I

    if-eqz v0, :cond_1

    new-array v0, v3, [I

    iget v1, p0, Lcom/kamcord/android/core/KC_y;->h:I

    aput v1, v0, v2

    invoke-static {v3, v0, v2}, Landroid/opengl/GLES20;->glDeleteRenderbuffers(I[II)V

    iput v2, p0, Lcom/kamcord/android/core/KC_y;->h:I

    :cond_1
    iget v0, p0, Lcom/kamcord/android/core/KC_y;->b:I

    if-eqz v0, :cond_2

    iget v0, p0, Lcom/kamcord/android/core/KC_y;->b:I

    invoke-virtual {p0, v0}, Lcom/kamcord/android/core/KC_y;->a(I)V

    iput v2, p0, Lcom/kamcord/android/core/KC_y;->b:I

    :cond_2
    return-void
.end method

.method protected a(I)V
    .locals 3

    const/4 v2, 0x1

    const/4 v1, 0x0

    new-array v0, v2, [I

    aput p1, v0, v1

    invoke-static {v2, v0, v1}, Landroid/opengl/GLES20;->glDeleteTextures(I[II)V

    return-void
.end method

.method final a(Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_p;)V
    .locals 10

    iget v1, p1, Lcom/kamcord/android/core/KC_f;->a:I

    iget v2, p1, Lcom/kamcord/android/core/KC_f;->b:I

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/core/KC_y;->c:I

    const/4 v0, 0x0

    iput v0, p0, Lcom/kamcord/android/core/KC_y;->d:I

    iput v1, p0, Lcom/kamcord/android/core/KC_y;->e:I

    iput v2, p0, Lcom/kamcord/android/core/KC_y;->f:I

    const v0, 0x8ca6

    iget-object v3, p0, Lcom/kamcord/android/core/KC_y;->i:[I

    const/4 v4, 0x0

    invoke-static {v0, v3, v4}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_y;->i:[I

    const/4 v3, 0x0

    aget v3, v0, v3

    const v0, 0x84e0

    iget-object v4, p0, Lcom/kamcord/android/core/KC_y;->i:[I

    const/4 v5, 0x0

    invoke-static {v0, v4, v5}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_y;->i:[I

    const/4 v4, 0x0

    aget v4, v0, v4

    const/4 v0, 0x1

    iget-object v5, p0, Lcom/kamcord/android/core/KC_y;->i:[I

    const/4 v6, 0x0

    invoke-static {v0, v5, v6}, Landroid/opengl/GLES20;->glGenFramebuffers(I[II)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_y;->i:[I

    const/4 v5, 0x0

    aget v0, v0, v5

    iput v0, p0, Lcom/kamcord/android/core/KC_y;->a:I

    const v0, 0x8d40

    iget v5, p0, Lcom/kamcord/android/core/KC_y;->a:I

    invoke-static {v0, v5}, Lcom/kamcord/android/core/KamcordNative;->glBindFramebuffer(II)V

    const v0, 0x84c0

    invoke-static {v0}, Landroid/opengl/GLES20;->glActiveTexture(I)V

    invoke-virtual {p0, v1, v2, p2}, Lcom/kamcord/android/core/KC_y;->a(IILcom/kamcord/android/core/KC_p;)I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/core/KC_y;->b:I

    const v0, 0x84c0

    invoke-static {v0}, Landroid/opengl/GLES20;->glActiveTexture(I)V

    const/16 v0, 0xde1

    iget v5, p0, Lcom/kamcord/android/core/KC_y;->b:I

    invoke-static {v0, v5}, Landroid/opengl/GLES20;->glBindTexture(II)V

    const v0, 0x8d40

    const v5, 0x8ce0

    const/16 v6, 0xde1

    iget v7, p0, Lcom/kamcord/android/core/KC_y;->b:I

    const/4 v8, 0x0

    invoke-static {v0, v5, v6, v7, v8}, Landroid/opengl/GLES20;->glFramebufferTexture2D(IIIII)V

    const v0, 0x8d40

    invoke-static {v0}, Landroid/opengl/GLES20;->glCheckFramebufferStatus(I)I

    move-result v0

    const v5, 0x8cd5

    if-eq v0, v5, :cond_0

    const-string v0, "OpenGL framebuffer generation failed\n"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    :cond_0
    iget v0, p2, Lcom/kamcord/android/core/KC_p;->a:I

    if-eqz v0, :cond_3

    const/4 v0, 0x1

    iget-object v5, p0, Lcom/kamcord/android/core/KC_y;->i:[I

    const/4 v6, 0x0

    invoke-static {v0, v5, v6}, Landroid/opengl/GLES20;->glGenRenderbuffers(I[II)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_y;->i:[I

    const/4 v5, 0x0

    aget v0, v0, v5

    iput v0, p0, Lcom/kamcord/android/core/KC_y;->h:I

    iget v5, p0, Lcom/kamcord/android/core/KC_y;->a:I

    iget v6, p0, Lcom/kamcord/android/core/KC_y;->h:I

    const/4 v0, 0x1

    new-array v0, v0, [I

    const v7, 0x8ca7

    const/4 v8, 0x0

    invoke-static {v7, v0, v8}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    const/4 v7, 0x0

    aget v7, v0, v7

    const v0, 0x81a5

    iget v8, p2, Lcom/kamcord/android/core/KC_p;->a:I

    const/16 v9, 0x18

    if-ne v8, v9, :cond_1

    const-string v0, "Using 24-bit depth buffer."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    const v0, 0x88f0

    :cond_1
    const v8, 0x8d40

    invoke-static {v8, v5}, Lcom/kamcord/android/core/KamcordNative;->glBindFramebuffer(II)V

    const v5, 0x8d41

    invoke-static {v5, v6}, Landroid/opengl/GLES20;->glBindRenderbuffer(II)V

    const v5, 0x8d41

    invoke-static {v5, v0, v1, v2}, Landroid/opengl/GLES20;->glRenderbufferStorage(IIII)V

    const v0, 0x8d40

    const v5, 0x8d00

    const v8, 0x8d41

    invoke-static {v0, v5, v8, v6}, Landroid/opengl/GLES20;->glFramebufferRenderbuffer(IIII)V

    iget v0, p2, Lcom/kamcord/android/core/KC_p;->a:I

    const/16 v5, 0x18

    if-ne v0, v5, :cond_7

    iget v0, p2, Lcom/kamcord/android/core/KC_p;->c:I

    const/16 v5, 0x8

    if-ne v0, v5, :cond_7

    const v0, 0x8d40

    const v1, 0x8d20

    const v2, 0x8d41

    invoke-static {v0, v1, v2, v6}, Landroid/opengl/GLES20;->glFramebufferRenderbuffer(IIII)V

    :cond_2
    :goto_0
    const v0, 0x8d41

    invoke-static {v0, v7}, Landroid/opengl/GLES20;->glBindRenderbuffer(II)V

    :cond_3
    invoke-static {}, Landroid/opengl/GLES20;->glGetError()I

    move-result v0

    if-eqz v0, :cond_4

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Creating framebuffer: glError = "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    :cond_4
    const/4 v0, 0x0

    const/4 v1, 0x0

    const/4 v2, 0x0

    const/high16 v5, 0x3f800000    # 1.0f

    invoke-static {v0, v1, v2, v5}, Landroid/opengl/GLES20;->glClearColor(FFFF)V

    const/16 v0, 0x4000

    iget v1, p2, Lcom/kamcord/android/core/KC_p;->a:I

    if-eqz v1, :cond_5

    const/16 v0, 0x4100

    :cond_5
    iget v1, p2, Lcom/kamcord/android/core/KC_p;->c:I

    if-eqz v1, :cond_6

    or-int/lit16 v0, v0, 0x400

    :cond_6
    invoke-static {v0}, Landroid/opengl/GLES20;->glClear(I)V

    invoke-static {v4}, Landroid/opengl/GLES20;->glActiveTexture(I)V

    const v0, 0x8d40

    invoke-static {v0, v3}, Lcom/kamcord/android/core/KamcordNative;->glBindFramebuffer(II)V

    return-void

    :cond_7
    iget v0, p2, Lcom/kamcord/android/core/KC_p;->c:I

    const/16 v5, 0x8

    if-ne v0, v5, :cond_2

    const/4 v0, 0x1

    new-array v0, v0, [I

    const/4 v5, 0x1

    const/4 v6, 0x0

    invoke-static {v5, v0, v6}, Landroid/opengl/GLES20;->glGenRenderbuffers(I[II)V

    const v5, 0x8d41

    const/4 v6, 0x0

    aget v6, v0, v6

    invoke-static {v5, v6}, Landroid/opengl/GLES20;->glBindRenderbuffer(II)V

    const v5, 0x8d41

    const v6, 0x8d48

    invoke-static {v5, v6, v1, v2}, Landroid/opengl/GLES20;->glRenderbufferStorage(IIII)V

    const v1, 0x8d40

    const v2, 0x8d20

    const v5, 0x8d41

    const/4 v6, 0x0

    aget v0, v0, v6

    invoke-static {v1, v2, v5, v0}, Landroid/opengl/GLES20;->glFramebufferRenderbuffer(IIII)V

    goto :goto_0
.end method
