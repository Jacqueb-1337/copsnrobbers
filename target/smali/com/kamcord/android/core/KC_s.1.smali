.class abstract Lcom/kamcord/android/core/KC_s;
.super Ljava/lang/Object;


# static fields
.field private static e:Lcom/kamcord/android/core/KC_m;


# instance fields
.field a:Lcom/kamcord/android/core/KC_x;

.field private b:Lcom/kamcord/android/core/KC_y;

.field private c:I

.field private d:Lcom/kamcord/android/core/KC_f;

.field private f:Z

.field private g:Lcom/kamcord/android/core/KC_y;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->d()Z

    move-result v0

    if-eqz v0, :cond_0

    new-instance v0, Lcom/kamcord/android/core/KC_o;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_o;-><init>()V

    sput-object v0, Lcom/kamcord/android/core/KC_s;->e:Lcom/kamcord/android/core/KC_m;

    :goto_0
    return-void

    :cond_0
    new-instance v0, Lcom/kamcord/android/core/KC_n;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_n;-><init>()V

    sput-object v0, Lcom/kamcord/android/core/KC_s;->e:Lcom/kamcord/android/core/KC_m;

    goto :goto_0
.end method

.method constructor <init>()V
    .locals 2

    const/4 v1, 0x0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_s;->f:Z

    new-instance v0, Lcom/kamcord/android/core/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_x;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_s;->a:Lcom/kamcord/android/core/KC_x;

    new-instance v0, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v0, v1, v1}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_s;->d:Lcom/kamcord/android/core/KC_f;

    new-instance v0, Lcom/kamcord/android/core/KC_y;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_y;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/core/KC_s;->b:Lcom/kamcord/android/core/KC_y;

    return-void
.end method

.method static a(I)Lcom/kamcord/android/core/KC_v;
    .locals 8

    const/16 v7, 0x201

    const v6, 0x812f

    const/16 v5, 0x2600

    const/16 v4, 0xde1

    const/4 v3, 0x0

    new-instance v0, Lcom/kamcord/android/core/KC_v;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_v;-><init>()V

    const/4 v1, 0x1

    new-array v1, v1, [I

    const v2, 0x84e0

    invoke-static {v2, v1, v3}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    aget v2, v1, v3

    iput v2, v0, Lcom/kamcord/android/core/KC_v;->a:I

    const v2, 0x84c0

    invoke-static {v2}, Landroid/opengl/GLES20;->glActiveTexture(I)V

    const v2, 0x8069

    invoke-static {v2, v1, v3}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    aget v2, v1, v3

    iput v2, v0, Lcom/kamcord/android/core/KC_v;->b:I

    const v2, 0x8b8d

    invoke-static {v2, v1, v3}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    aget v2, v1, v3

    iput v2, v0, Lcom/kamcord/android/core/KC_v;->h:I

    const v2, 0x8895

    invoke-static {v2, v1, v3}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    aget v2, v1, v3

    iput v2, v0, Lcom/kamcord/android/core/KC_v;->p:I

    const v2, 0x8894

    invoke-static {v2, v1, v3}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    aget v2, v1, v3

    iput v2, v0, Lcom/kamcord/android/core/KC_v;->o:I

    const/16 v2, 0xbe2

    invoke-static {v2}, Landroid/opengl/GLES20;->glIsEnabled(I)Z

    move-result v2

    iput-boolean v2, v0, Lcom/kamcord/android/core/KC_v;->k:Z

    iget-boolean v2, v0, Lcom/kamcord/android/core/KC_v;->k:Z

    if-eqz v2, :cond_0

    const/16 v2, 0xbe2

    invoke-static {v2}, Landroid/opengl/GLES20;->glDisable(I)V

    :cond_0
    const/16 v2, 0xb44

    invoke-static {v2}, Landroid/opengl/GLES20;->glIsEnabled(I)Z

    move-result v2

    iput-boolean v2, v0, Lcom/kamcord/android/core/KC_v;->l:Z

    iget-boolean v2, v0, Lcom/kamcord/android/core/KC_v;->l:Z

    if-eqz v2, :cond_1

    const/16 v2, 0xb44

    invoke-static {v2}, Landroid/opengl/GLES20;->glDisable(I)V

    :cond_1
    const/16 v2, 0xb71

    invoke-static {v2}, Landroid/opengl/GLES20;->glIsEnabled(I)Z

    move-result v2

    iput-boolean v2, v0, Lcom/kamcord/android/core/KC_v;->m:Z

    iget-boolean v2, v0, Lcom/kamcord/android/core/KC_v;->m:Z

    if-eqz v2, :cond_2

    const/16 v2, 0xb71

    invoke-static {v2}, Landroid/opengl/GLES20;->glDisable(I)V

    :cond_2
    const/16 v2, 0xb90

    invoke-static {v2}, Landroid/opengl/GLES20;->glIsEnabled(I)Z

    move-result v2

    iput-boolean v2, v0, Lcom/kamcord/android/core/KC_v;->n:Z

    iget-boolean v2, v0, Lcom/kamcord/android/core/KC_v;->n:Z

    if-eqz v2, :cond_3

    const/16 v2, 0xb90

    invoke-static {v2}, Landroid/opengl/GLES20;->glDisable(I)V

    :cond_3
    const/16 v2, 0xb74

    invoke-static {v2, v1, v3}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    aget v2, v1, v3

    iput v2, v0, Lcom/kamcord/android/core/KC_v;->i:I

    iget v2, v0, Lcom/kamcord/android/core/KC_v;->i:I

    if-eq v2, v7, :cond_4

    invoke-static {v7}, Landroid/opengl/GLES20;->glDepthFunc(I)V

    :cond_4
    if-eqz p0, :cond_8

    invoke-static {v4, p0}, Landroid/opengl/GLES20;->glBindTexture(II)V

    const/16 v2, 0x2801

    invoke-static {v4, v2, v1, v3}, Landroid/opengl/GLES20;->glGetTexParameteriv(II[II)V

    aget v2, v1, v3

    iput v2, v0, Lcom/kamcord/android/core/KC_v;->c:I

    iget v2, v0, Lcom/kamcord/android/core/KC_v;->c:I

    if-eq v2, v5, :cond_5

    const/16 v2, 0x2801

    invoke-static {v4, v2, v5}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_5
    const/16 v2, 0x2800

    invoke-static {v4, v2, v1, v3}, Landroid/opengl/GLES20;->glGetTexParameteriv(II[II)V

    aget v2, v1, v3

    iput v2, v0, Lcom/kamcord/android/core/KC_v;->d:I

    iget v2, v0, Lcom/kamcord/android/core/KC_v;->d:I

    if-eq v2, v5, :cond_6

    const/16 v2, 0x2800

    invoke-static {v4, v2, v5}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_6
    const/16 v2, 0x2802

    invoke-static {v4, v2, v1, v3}, Landroid/opengl/GLES20;->glGetTexParameteriv(II[II)V

    aget v2, v1, v3

    iput v2, v0, Lcom/kamcord/android/core/KC_v;->e:I

    iget v2, v0, Lcom/kamcord/android/core/KC_v;->e:I

    if-eq v2, v6, :cond_7

    const/16 v2, 0x2802

    invoke-static {v4, v2, v6}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_7
    const/16 v2, 0x2803

    invoke-static {v4, v2, v1, v3}, Landroid/opengl/GLES20;->glGetTexParameteriv(II[II)V

    aget v2, v1, v3

    iput v2, v0, Lcom/kamcord/android/core/KC_v;->f:I

    iget v2, v0, Lcom/kamcord/android/core/KC_v;->f:I

    if-eq v2, v6, :cond_8

    const/16 v2, 0x2803

    invoke-static {v4, v2, v6}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_8
    const v2, 0x8ca6

    invoke-static {v2, v1, v3}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    aget v1, v1, v3

    iput v1, v0, Lcom/kamcord/android/core/KC_v;->g:I

    const/16 v1, 0xba2

    iget-object v2, v0, Lcom/kamcord/android/core/KC_v;->j:[I

    invoke-static {v1, v2, v3}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    return-object v0
.end method

.method static a(Lcom/kamcord/android/core/KC_v;Z)V
    .locals 8

    const v7, 0x812f

    const/16 v6, 0x2600

    const/16 v5, 0xde1

    const v0, 0x8d40

    iget v1, p0, Lcom/kamcord/android/core/KC_v;->g:I

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KamcordNative;->glBindFramebuffer(II)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_v;->j:[I

    const/4 v1, 0x0

    aget v0, v0, v1

    iget-object v1, p0, Lcom/kamcord/android/core/KC_v;->j:[I

    const/4 v2, 0x1

    aget v1, v1, v2

    iget-object v2, p0, Lcom/kamcord/android/core/KC_v;->j:[I

    const/4 v3, 0x2

    aget v2, v2, v3

    iget-object v3, p0, Lcom/kamcord/android/core/KC_v;->j:[I

    const/4 v4, 0x3

    aget v3, v3, v4

    invoke-static {v0, v1, v2, v3}, Landroid/opengl/GLES20;->glViewport(IIII)V

    if-eqz p1, :cond_3

    iget v0, p0, Lcom/kamcord/android/core/KC_v;->f:I

    if-eq v0, v7, :cond_0

    const/16 v0, 0x2803

    iget v1, p0, Lcom/kamcord/android/core/KC_v;->f:I

    invoke-static {v5, v0, v1}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_0
    iget v0, p0, Lcom/kamcord/android/core/KC_v;->e:I

    if-eq v0, v7, :cond_1

    const/16 v0, 0x2802

    iget v1, p0, Lcom/kamcord/android/core/KC_v;->e:I

    invoke-static {v5, v0, v1}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_1
    iget v0, p0, Lcom/kamcord/android/core/KC_v;->d:I

    if-eq v0, v6, :cond_2

    const/16 v0, 0x2800

    iget v1, p0, Lcom/kamcord/android/core/KC_v;->d:I

    invoke-static {v5, v0, v1}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_2
    iget v0, p0, Lcom/kamcord/android/core/KC_v;->c:I

    if-eq v0, v6, :cond_3

    const/16 v0, 0x2801

    iget v1, p0, Lcom/kamcord/android/core/KC_v;->c:I

    invoke-static {v5, v0, v1}, Landroid/opengl/GLES20;->glTexParameteri(III)V

    :cond_3
    iget v0, p0, Lcom/kamcord/android/core/KC_v;->i:I

    const/16 v1, 0x201

    if-eq v0, v1, :cond_4

    iget v0, p0, Lcom/kamcord/android/core/KC_v;->i:I

    invoke-static {v0}, Landroid/opengl/GLES20;->glDepthFunc(I)V

    :cond_4
    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_v;->n:Z

    if-eqz v0, :cond_5

    const/16 v0, 0xb90

    invoke-static {v0}, Landroid/opengl/GLES20;->glEnable(I)V

    :cond_5
    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_v;->m:Z

    if-eqz v0, :cond_6

    const/16 v0, 0xb71

    invoke-static {v0}, Landroid/opengl/GLES20;->glEnable(I)V

    :cond_6
    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_v;->l:Z

    if-eqz v0, :cond_7

    const/16 v0, 0xb44

    invoke-static {v0}, Landroid/opengl/GLES20;->glEnable(I)V

    :cond_7
    iget-boolean v0, p0, Lcom/kamcord/android/core/KC_v;->k:Z

    if-eqz v0, :cond_8

    const/16 v0, 0xbe2

    invoke-static {v0}, Landroid/opengl/GLES20;->glEnable(I)V

    :cond_8
    iget v0, p0, Lcom/kamcord/android/core/KC_v;->h:I

    invoke-static {v0}, Landroid/opengl/GLES20;->glUseProgram(I)V

    const v0, 0x8893

    iget v1, p0, Lcom/kamcord/android/core/KC_v;->p:I

    invoke-static {v0, v1}, Landroid/opengl/GLES20;->glBindBuffer(II)V

    const v0, 0x8892

    iget v1, p0, Lcom/kamcord/android/core/KC_v;->o:I

    invoke-static {v0, v1}, Landroid/opengl/GLES20;->glBindBuffer(II)V

    iget v0, p0, Lcom/kamcord/android/core/KC_v;->b:I

    invoke-static {v5, v0}, Landroid/opengl/GLES20;->glBindTexture(II)V

    iget v0, p0, Lcom/kamcord/android/core/KC_v;->a:I

    const v1, 0x84c0

    if-eq v0, v1, :cond_9

    iget v0, p0, Lcom/kamcord/android/core/KC_v;->a:I

    invoke-static {v0}, Landroid/opengl/GLES20;->glActiveTexture(I)V

    :cond_9
    return-void
.end method

.method static f()Lcom/kamcord/android/core/KC_f;
    .locals 1

    sget-object v0, Lcom/kamcord/android/core/KC_s;->e:Lcom/kamcord/android/core/KC_m;

    invoke-interface {v0}, Lcom/kamcord/android/core/KC_m;->a()Lcom/kamcord/android/core/KC_f;

    move-result-object v0

    return-object v0
.end method

.method static i()Landroid/graphics/Bitmap;
    .locals 8

    const/4 v0, 0x0

    invoke-static {}, Lcom/kamcord/android/core/KC_s;->f()Lcom/kamcord/android/core/KC_f;

    move-result-object v1

    iget v2, v1, Lcom/kamcord/android/core/KC_f;->a:I

    iget v3, v1, Lcom/kamcord/android/core/KC_f;->b:I

    mul-int/lit8 v1, v2, 0x4

    mul-int/2addr v1, v3

    invoke-static {v1}, Ljava/nio/ByteBuffer;->allocateDirect(I)Ljava/nio/ByteBuffer;

    move-result-object v6

    const/16 v4, 0x1908

    const/16 v5, 0x1401

    move v1, v0

    invoke-static/range {v0 .. v6}, Landroid/opengl/GLES20;->glReadPixels(IIIIIILjava/nio/Buffer;)V

    sget-object v1, Landroid/graphics/Bitmap$Config;->ARGB_8888:Landroid/graphics/Bitmap$Config;

    invoke-static {v2, v3, v1}, Landroid/graphics/Bitmap;->createBitmap(IILandroid/graphics/Bitmap$Config;)Landroid/graphics/Bitmap;

    move-result-object v1

    invoke-virtual {v1, v6}, Landroid/graphics/Bitmap;->copyPixelsFromBuffer(Ljava/nio/Buffer;)V

    new-instance v6, Landroid/graphics/Matrix;

    invoke-direct {v6}, Landroid/graphics/Matrix;-><init>()V

    const/high16 v2, 0x3f800000    # 1.0f

    const/high16 v3, -0x40800000    # -1.0f

    invoke-virtual {v6, v2, v3}, Landroid/graphics/Matrix;->preScale(FF)Z

    invoke-virtual {v1}, Landroid/graphics/Bitmap;->getWidth()I

    move-result v4

    invoke-virtual {v1}, Landroid/graphics/Bitmap;->getHeight()I

    move-result v5

    move v2, v0

    move v3, v0

    move v7, v0

    invoke-static/range {v1 .. v7}, Landroid/graphics/Bitmap;->createBitmap(Landroid/graphics/Bitmap;IIIILandroid/graphics/Matrix;Z)Landroid/graphics/Bitmap;

    move-result-object v0

    return-object v0
.end method


# virtual methods
.method final a(Lcom/kamcord/android/core/KC_f;)I
    .locals 3

    sget-object v0, Lcom/kamcord/android/core/KC_s;->e:Lcom/kamcord/android/core/KC_m;

    invoke-interface {v0}, Lcom/kamcord/android/core/KC_m;->c()I

    move-result v1

    const/4 v0, 0x0

    iget-boolean v2, p0, Lcom/kamcord/android/core/KC_s;->f:Z

    if-eqz v2, :cond_0

    const/4 v0, 0x1

    :cond_0
    iget v2, p0, Lcom/kamcord/android/core/KC_s;->c:I

    if-eq v1, v2, :cond_1

    or-int/lit8 v0, v0, 0x2

    :cond_1
    iget-object v2, p0, Lcom/kamcord/android/core/KC_s;->d:Lcom/kamcord/android/core/KC_f;

    invoke-virtual {v2, p1}, Lcom/kamcord/android/core/KC_f;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-nez v2, :cond_2

    or-int/lit8 v0, v0, 0x4

    :cond_2
    iput v1, p0, Lcom/kamcord/android/core/KC_s;->c:I

    iget-object v1, p0, Lcom/kamcord/android/core/KC_s;->d:Lcom/kamcord/android/core/KC_f;

    iget v2, p1, Lcom/kamcord/android/core/KC_f;->a:I

    iput v2, v1, Lcom/kamcord/android/core/KC_f;->a:I

    iget v2, p1, Lcom/kamcord/android/core/KC_f;->b:I

    iput v2, v1, Lcom/kamcord/android/core/KC_f;->b:I

    return v0
.end method

.method abstract a()V
.end method

.method final a(Lcom/kamcord/android/core/KC_f;I)V
    .locals 3

    const/4 v2, 0x0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_s;->b:Lcom/kamcord/android/core/KC_y;

    iput v2, v0, Lcom/kamcord/android/core/KC_y;->c:I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_s;->b:Lcom/kamcord/android/core/KC_y;

    iput v2, v0, Lcom/kamcord/android/core/KC_y;->d:I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_s;->b:Lcom/kamcord/android/core/KC_y;

    iget v1, p1, Lcom/kamcord/android/core/KC_f;->a:I

    iput v1, v0, Lcom/kamcord/android/core/KC_y;->e:I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_s;->b:Lcom/kamcord/android/core/KC_y;

    iget v1, p1, Lcom/kamcord/android/core/KC_f;->b:I

    iput v1, v0, Lcom/kamcord/android/core/KC_y;->f:I

    and-int/lit8 v0, p2, 0x4

    if-eqz v0, :cond_0

    and-int/lit8 v0, p2, 0x2

    if-nez v0, :cond_0

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_s;->e()V

    :cond_0
    invoke-static {v2}, Lcom/kamcord/android/core/KC_s;->a(I)Lcom/kamcord/android/core/KC_v;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/core/KC_s;->e:Lcom/kamcord/android/core/KC_m;

    invoke-interface {v1}, Lcom/kamcord/android/core/KC_m;->b()Lcom/kamcord/android/core/KC_p;

    move-result-object v1

    invoke-virtual {p0, p1, v1}, Lcom/kamcord/android/core/KC_s;->a(Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_p;)V

    iget-object v1, p0, Lcom/kamcord/android/core/KC_s;->a:Lcom/kamcord/android/core/KC_x;

    invoke-virtual {v1}, Lcom/kamcord/android/core/KC_x;->a()Z

    invoke-static {v0, v2}, Lcom/kamcord/android/core/KC_s;->a(Lcom/kamcord/android/core/KC_v;Z)V

    iput-boolean v2, p0, Lcom/kamcord/android/core/KC_s;->f:Z

    return-void
.end method

.method protected abstract a(Lcom/kamcord/android/core/KC_f;Lcom/kamcord/android/core/KC_p;)V
.end method

.method abstract b()V
.end method

.method protected abstract c()Lcom/kamcord/android/core/KC_y;
.end method

.method abstract d()Lcom/kamcord/android/core/KC_y;
.end method

.method protected abstract e()V
.end method

.method final g()V
    .locals 1

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_s;->e()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_s;->a:Lcom/kamcord/android/core/KC_x;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_x;->b()V

    return-void
.end method

.method final h()Z
    .locals 2

    invoke-virtual {p0}, Lcom/kamcord/android/core/KC_s;->c()Lcom/kamcord/android/core/KC_y;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_s;->g:Lcom/kamcord/android/core/KC_y;

    iget-object v0, p0, Lcom/kamcord/android/core/KC_s;->g:Lcom/kamcord/android/core/KC_y;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_s;->g:Lcom/kamcord/android/core/KC_y;

    sget-object v1, Lcom/kamcord/android/core/KC_u;->b:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    iget v0, v0, Lcom/kamcord/android/core/KC_y;->a:I

    invoke-static {v0}, Lcom/kamcord/android/core/KamcordNative;->setDefaultFramebuffer(I)V

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method final j()V
    .locals 5

    iget-object v0, p0, Lcom/kamcord/android/core/KC_s;->a:Lcom/kamcord/android/core/KC_x;

    const/4 v1, 0x1

    const/4 v2, 0x0

    iget-object v3, p0, Lcom/kamcord/android/core/KC_s;->g:Lcom/kamcord/android/core/KC_y;

    iget-object v4, p0, Lcom/kamcord/android/core/KC_s;->b:Lcom/kamcord/android/core/KC_y;

    invoke-virtual {v0, v1, v2, v3, v4}, Lcom/kamcord/android/core/KC_x;->a(ZZLcom/kamcord/android/core/KC_y;Lcom/kamcord/android/core/KC_y;)V

    return-void
.end method
