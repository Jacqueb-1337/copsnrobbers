.class final Lcom/kamcord/android/core/KC_x;
.super Ljava/lang/Object;


# instance fields
.field private a:I

.field private b:I

.field private c:I

.field private d:I

.field private e:I

.field private f:I

.field private g:I

.field private h:I


# direct methods
.method constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method

.method private static a(ILjava/lang/String;)I
    .locals 4

    const/4 v0, 0x0

    const/4 v1, 0x1

    new-array v2, v1, [I

    if-nez p1, :cond_0

    :goto_0
    return v0

    :cond_0
    invoke-static {p0}, Landroid/opengl/GLES20;->glCreateShader(I)I

    move-result v1

    invoke-static {v1, p1}, Landroid/opengl/GLES20;->glShaderSource(ILjava/lang/String;)V

    invoke-static {v1}, Landroid/opengl/GLES20;->glCompileShader(I)V

    const v3, 0x8b81

    invoke-static {v1, v3, v2, v0}, Landroid/opengl/GLES20;->glGetShaderiv(II[II)V

    aget v2, v2, v0

    if-nez v2, :cond_1

    const-string v2, "Shader compile status non-zero.\n"

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    invoke-static {v1}, Landroid/opengl/GLES20;->glGetShaderInfoLog(I)Ljava/lang/String;

    move-result-object v1

    new-instance v2, Ljava/lang/StringBuilder;

    invoke-direct {v2}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v2, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "\n"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    goto :goto_0

    :cond_1
    move v0, v1

    goto :goto_0
.end method


# virtual methods
.method final a(ZZLcom/kamcord/android/core/KC_y;Lcom/kamcord/android/core/KC_y;)V
    .locals 11

    const/4 v0, 0x0

    const v10, 0x8892

    const/4 v9, 0x5

    const/4 v6, 0x1

    const/4 v3, 0x0

    if-eqz p1, :cond_4

    iget v0, p3, Lcom/kamcord/android/core/KC_y;->b:I

    invoke-static {v0}, Lcom/kamcord/android/core/KC_s;->a(I)Lcom/kamcord/android/core/KC_v;

    move-result-object v1

    iget v4, p0, Lcom/kamcord/android/core/KC_x;->d:I

    new-instance v2, Lcom/kamcord/android/core/KC_G;

    invoke-direct {v2}, Lcom/kamcord/android/core/KC_G;-><init>()V

    new-array v5, v6, [I

    const v0, 0x8622

    invoke-static {v4, v0, v5, v3}, Landroid/opengl/GLES20;->glGetVertexAttribiv(II[II)V

    aget v0, v5, v3

    if-eqz v0, :cond_2

    move v0, v6

    :goto_0
    iput-boolean v0, v2, Lcom/kamcord/android/core/KC_G;->a:Z

    const v0, 0x8623

    invoke-static {v4, v0, v5, v3}, Landroid/opengl/GLES20;->glGetVertexAttribiv(II[II)V

    const v0, 0x8625

    invoke-static {v4, v0, v5, v3}, Landroid/opengl/GLES20;->glGetVertexAttribiv(II[II)V

    const v0, 0x886a

    invoke-static {v4, v0, v5, v3}, Landroid/opengl/GLES20;->glGetVertexAttribiv(II[II)V

    const v0, 0x8624

    invoke-static {v4, v0, v5, v3}, Landroid/opengl/GLES20;->glGetVertexAttribiv(II[II)V

    const v0, 0x889f

    invoke-static {v4, v0, v5, v3}, Landroid/opengl/GLES20;->glGetVertexAttribiv(II[II)V

    aget v0, v5, v3

    iput v0, v2, Lcom/kamcord/android/core/KC_G;->b:I

    move-object v7, v1

    move-object v8, v2

    :goto_1
    const v0, 0x8d40

    iget v1, p4, Lcom/kamcord/android/core/KC_y;->a:I

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KamcordNative;->glBindFramebuffer(II)V

    iget v0, p4, Lcom/kamcord/android/core/KC_y;->e:I

    iget v1, p4, Lcom/kamcord/android/core/KC_y;->f:I

    invoke-static {v3, v3, v0, v1}, Landroid/opengl/GLES20;->glViewport(IIII)V

    const v0, 0x84c0

    invoke-static {v0}, Landroid/opengl/GLES20;->glActiveTexture(I)V

    const/16 v0, 0xde1

    iget v1, p3, Lcom/kamcord/android/core/KC_y;->b:I

    invoke-static {v0, v1}, Landroid/opengl/GLES20;->glBindTexture(II)V

    iget v0, p0, Lcom/kamcord/android/core/KC_x;->a:I

    invoke-static {v0}, Landroid/opengl/GLES20;->glUseProgram(I)V

    iget v0, p0, Lcom/kamcord/android/core/KC_x;->g:I

    invoke-static {v10, v0}, Landroid/opengl/GLES20;->glBindBuffer(II)V

    const v0, 0x8893

    iget v1, p0, Lcom/kamcord/android/core/KC_x;->h:I

    invoke-static {v0, v1}, Landroid/opengl/GLES20;->glBindBuffer(II)V

    iget v0, p0, Lcom/kamcord/android/core/KC_x;->d:I

    invoke-static {v0}, Landroid/opengl/GLES20;->glEnableVertexAttribArray(I)V

    iget v0, p0, Lcom/kamcord/android/core/KC_x;->d:I

    const/4 v1, 0x2

    const/16 v2, 0x1406

    move v4, v3

    move v5, v3

    invoke-static/range {v0 .. v5}, Landroid/opengl/GLES20;->glVertexAttribPointer(IIIZII)V

    const/16 v0, 0x10

    new-array v0, v0, [F

    fill-array-data v0, :array_0

    if-eqz p2, :cond_0

    const/high16 v1, -0x40800000    # -1.0f

    aput v1, v0, v9

    :cond_0
    iget v1, p0, Lcom/kamcord/android/core/KC_x;->f:I

    invoke-static {v1, v6, v3, v0, v3}, Landroid/opengl/GLES20;->glUniformMatrix4fv(IIZ[FI)V

    iget v0, p0, Lcom/kamcord/android/core/KC_x;->e:I

    invoke-static {v0, v3}, Landroid/opengl/GLES20;->glUniform1i(II)V

    const/4 v0, 0x4

    invoke-static {v9, v3, v0}, Landroid/opengl/GLES20;->glDrawArrays(III)V

    if-eqz p1, :cond_1

    iget v0, p0, Lcom/kamcord/android/core/KC_x;->d:I

    iget-boolean v1, v8, Lcom/kamcord/android/core/KC_G;->a:Z

    if-eqz v1, :cond_3

    invoke-static {v0}, Landroid/opengl/GLES20;->glEnableVertexAttribArray(I)V

    :goto_2
    new-array v0, v6, [I

    const v1, 0x8894

    invoke-static {v1, v0, v3}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    iget v0, v8, Lcom/kamcord/android/core/KC_G;->b:I

    invoke-static {v10, v0}, Landroid/opengl/GLES20;->glBindBuffer(II)V

    invoke-static {v7, v6}, Lcom/kamcord/android/core/KC_s;->a(Lcom/kamcord/android/core/KC_v;Z)V

    :cond_1
    return-void

    :cond_2
    move v0, v3

    goto/16 :goto_0

    :cond_3
    invoke-static {v0}, Landroid/opengl/GLES20;->glDisableVertexAttribArray(I)V

    goto :goto_2

    :cond_4
    move-object v7, v0

    move-object v8, v0

    goto :goto_1

    nop

    :array_0
    .array-data 4
        0x3f800000    # 1.0f
        0x0
        0x0
        0x0
        0x0
        0x3f800000    # 1.0f
        0x0
        0x0
        0x0
        0x0
        0x3f800000    # 1.0f
        0x0
        0x0
        0x0
        0x0
        0x3f800000    # 1.0f
    .end array-data
.end method

.method final a()Z
    .locals 13

    const/4 v12, 0x4

    const v11, 0x8893

    const v10, 0x8892

    const/4 v1, 0x1

    const/4 v0, 0x0

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "\n"

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    const-string v3, "attribute vec4 a_position;\nuniform mat4 u_matrix;\n"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "\n\n"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "#ifdef GL_ES\nvarying mediump vec2 v_texCoord;\n"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "#else\nvarying vec2 v_texCoord;\n"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "#endif\n\n"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "void main()\n{\n"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "    gl_Position = u_matrix * a_position;\n    v_texCoord =\n"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "        0.5*(a_position.xy+vec2(1.0, 1.0));\n}\n"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "\n"

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    const-string v4, "#ifdef GL_ES\nprecision lowp float;\n"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, "#endif\n\n"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, "varying vec2 v_texCoord;\nuniform sampler2D u_texture;\n"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, "\nvoid main()\n"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, "{\n   gl_FragColor = vec4( texture2D(u_texture,\n"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, "        v_texCoord).xyz, 1.0 );\n}\n"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-static {}, Landroid/opengl/GLES20;->glCreateProgram()I

    move-result v4

    iput v4, p0, Lcom/kamcord/android/core/KC_x;->a:I

    const v4, 0x8b31

    invoke-static {v4, v2}, Lcom/kamcord/android/core/KC_x;->a(ILjava/lang/String;)I

    move-result v2

    iput v2, p0, Lcom/kamcord/android/core/KC_x;->b:I

    iget v2, p0, Lcom/kamcord/android/core/KC_x;->b:I

    if-nez v2, :cond_1

    :cond_0
    :goto_0
    return v0

    :cond_1
    const v2, 0x8b30

    invoke-static {v2, v3}, Lcom/kamcord/android/core/KC_x;->a(ILjava/lang/String;)I

    move-result v2

    iput v2, p0, Lcom/kamcord/android/core/KC_x;->c:I

    iget v2, p0, Lcom/kamcord/android/core/KC_x;->c:I

    if-eqz v2, :cond_0

    iget v2, p0, Lcom/kamcord/android/core/KC_x;->a:I

    iget v3, p0, Lcom/kamcord/android/core/KC_x;->b:I

    invoke-static {v2, v3}, Landroid/opengl/GLES20;->glAttachShader(II)V

    iget v2, p0, Lcom/kamcord/android/core/KC_x;->a:I

    iget v3, p0, Lcom/kamcord/android/core/KC_x;->c:I

    invoke-static {v2, v3}, Landroid/opengl/GLES20;->glAttachShader(II)V

    new-array v2, v1, [I

    const v3, 0x8869

    invoke-static {v3, v2, v0}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    aget v2, v2, v0

    add-int/lit8 v2, v2, -0x1

    iput v2, p0, Lcom/kamcord/android/core/KC_x;->d:I

    iget v2, p0, Lcom/kamcord/android/core/KC_x;->a:I

    iget v3, p0, Lcom/kamcord/android/core/KC_x;->d:I

    const-string v4, "a_position"

    invoke-static {v2, v3, v4}, Landroid/opengl/GLES20;->glBindAttribLocation(IILjava/lang/String;)V

    iget v2, p0, Lcom/kamcord/android/core/KC_x;->a:I

    invoke-static {v2}, Landroid/opengl/GLES20;->glLinkProgram(I)V

    new-array v3, v1, [I

    const v4, 0x8b82

    invoke-static {v2, v4, v3, v0}, Landroid/opengl/GLES20;->glGetProgramiv(II[II)V

    aget v4, v3, v0

    if-eq v4, v1, :cond_5

    const-string v3, "Shader link status non-true.\n"

    invoke-static {v3}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    invoke-static {v2}, Landroid/opengl/GLES20;->glGetProgramInfoLog(I)Ljava/lang/String;

    move-result-object v2

    new-instance v3, Ljava/lang/StringBuilder;

    invoke-direct {v3}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v3, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "\n"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :cond_2
    :goto_1
    iget v2, p0, Lcom/kamcord/android/core/KC_x;->b:I

    if-eqz v2, :cond_3

    iget v2, p0, Lcom/kamcord/android/core/KC_x;->b:I

    invoke-static {v2}, Landroid/opengl/GLES20;->glDeleteShader(I)V

    iput v0, p0, Lcom/kamcord/android/core/KC_x;->b:I

    :cond_3
    iget v2, p0, Lcom/kamcord/android/core/KC_x;->c:I

    if-eqz v2, :cond_4

    iget v2, p0, Lcom/kamcord/android/core/KC_x;->c:I

    invoke-static {v2}, Landroid/opengl/GLES20;->glDeleteShader(I)V

    iput v0, p0, Lcom/kamcord/android/core/KC_x;->c:I

    :cond_4
    new-array v2, v1, [I

    const v3, 0x8b8d

    invoke-static {v3, v2, v0}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    iget v3, p0, Lcom/kamcord/android/core/KC_x;->a:I

    invoke-static {v3}, Landroid/opengl/GLES20;->glUseProgram(I)V

    iget v3, p0, Lcom/kamcord/android/core/KC_x;->a:I

    const-string v4, "u_texture"

    invoke-static {v3, v4}, Landroid/opengl/GLES20;->glGetUniformLocation(ILjava/lang/String;)I

    move-result v3

    iput v3, p0, Lcom/kamcord/android/core/KC_x;->e:I

    iget v3, p0, Lcom/kamcord/android/core/KC_x;->a:I

    const-string v4, "u_matrix"

    invoke-static {v3, v4}, Landroid/opengl/GLES20;->glGetUniformLocation(ILjava/lang/String;)I

    move-result v3

    iput v3, p0, Lcom/kamcord/android/core/KC_x;->f:I

    new-array v3, v1, [I

    const v4, 0x8894

    invoke-static {v4, v3, v0}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    new-array v4, v1, [I

    const v5, 0x8895

    invoke-static {v5, v4, v0}, Lcom/kamcord/android/core/KamcordNative;->glGetIntegerv(I[II)V

    new-array v5, v1, [I

    invoke-static {v1, v5, v0}, Landroid/opengl/GLES20;->glGenBuffers(I[II)V

    aget v6, v5, v0

    iput v6, p0, Lcom/kamcord/android/core/KC_x;->g:I

    const/16 v6, 0x8

    new-array v6, v6, [F

    fill-array-data v6, :array_0

    invoke-static {v6}, Ljava/nio/FloatBuffer;->wrap([F)Ljava/nio/FloatBuffer;

    move-result-object v6

    new-array v7, v12, [B

    fill-array-data v7, :array_1

    invoke-static {v7}, Ljava/nio/ByteBuffer;->wrap([B)Ljava/nio/ByteBuffer;

    move-result-object v7

    iget v8, p0, Lcom/kamcord/android/core/KC_x;->g:I

    invoke-static {v10, v8}, Landroid/opengl/GLES20;->glBindBuffer(II)V

    const/16 v8, 0x20

    const v9, 0x88e4

    invoke-static {v10, v8, v6, v9}, Landroid/opengl/GLES20;->glBufferData(IILjava/nio/Buffer;I)V

    invoke-static {v1, v5, v0}, Landroid/opengl/GLES20;->glGenBuffers(I[II)V

    aget v5, v5, v0

    iput v5, p0, Lcom/kamcord/android/core/KC_x;->h:I

    iget v5, p0, Lcom/kamcord/android/core/KC_x;->h:I

    invoke-static {v11, v5}, Landroid/opengl/GLES20;->glBindBuffer(II)V

    const v5, 0x88e4

    invoke-static {v11, v12, v7, v5}, Landroid/opengl/GLES20;->glBufferData(IILjava/nio/Buffer;I)V

    aget v3, v3, v0

    invoke-static {v10, v3}, Landroid/opengl/GLES20;->glBindBuffer(II)V

    aget v3, v4, v0

    invoke-static {v11, v3}, Landroid/opengl/GLES20;->glBindBuffer(II)V

    aget v0, v2, v0

    invoke-static {v0}, Landroid/opengl/GLES20;->glUseProgram(I)V

    move v0, v1

    goto/16 :goto_0

    :cond_5
    invoke-static {v2}, Landroid/opengl/GLES20;->glValidateProgram(I)V

    const v4, 0x8b83

    invoke-static {v2, v4, v3, v0}, Landroid/opengl/GLES20;->glGetProgramiv(II[II)V

    aget v3, v3, v0

    if-eq v3, v1, :cond_2

    const-string v3, "Shader validation failure.\n"

    invoke-static {v3}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    invoke-static {v2}, Landroid/opengl/GLES20;->glGetProgramInfoLog(I)Ljava/lang/String;

    move-result-object v2

    new-instance v3, Ljava/lang/StringBuilder;

    invoke-direct {v3}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v3, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "\n"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    goto/16 :goto_1

    :array_0
    .array-data 4
        -0x40800000    # -1.0f
        -0x40800000    # -1.0f
        0x3f800000    # 1.0f
        -0x40800000    # -1.0f
        -0x40800000    # -1.0f
        0x3f800000    # 1.0f
        0x3f800000    # 1.0f
        0x3f800000    # 1.0f
    .end array-data

    :array_1
    .array-data 1
        0x0t
        0x1t
        0x2t
        0x3t
    .end array-data
.end method

.method final b()V
    .locals 4

    const/4 v3, 0x1

    const/4 v2, 0x0

    iget v0, p0, Lcom/kamcord/android/core/KC_x;->a:I

    if-eqz v0, :cond_0

    iget v0, p0, Lcom/kamcord/android/core/KC_x;->a:I

    invoke-static {v0}, Landroid/opengl/GLES20;->glDeleteProgram(I)V

    iput v2, p0, Lcom/kamcord/android/core/KC_x;->a:I

    :cond_0
    iget v0, p0, Lcom/kamcord/android/core/KC_x;->g:I

    if-eqz v0, :cond_1

    new-array v0, v3, [I

    iget v1, p0, Lcom/kamcord/android/core/KC_x;->g:I

    aput v1, v0, v2

    invoke-static {v3, v0, v2}, Landroid/opengl/GLES20;->glDeleteBuffers(I[II)V

    :cond_1
    iget v0, p0, Lcom/kamcord/android/core/KC_x;->h:I

    if-eqz v0, :cond_2

    new-array v0, v3, [I

    iget v1, p0, Lcom/kamcord/android/core/KC_x;->h:I

    aput v1, v0, v2

    invoke-static {v3, v0, v2}, Landroid/opengl/GLES20;->glDeleteBuffers(I[II)V

    :cond_2
    return-void
.end method
