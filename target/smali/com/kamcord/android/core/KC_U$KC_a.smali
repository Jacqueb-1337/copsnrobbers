.class final Lcom/kamcord/android/core/KC_U$KC_a;
.super Lcom/kamcord/android/KC_ac;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/core/KC_U;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_a"
.end annotation


# instance fields
.field private synthetic b:Lcom/kamcord/android/core/KC_U;


# direct methods
.method protected constructor <init>(Lcom/kamcord/android/core/KC_U;Landroid/os/Looper;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-direct {p0, p2}, Lcom/kamcord/android/KC_ac;-><init>(Landroid/os/Looper;)V

    return-void
.end method

.method private a()V
    .locals 6

    const/4 v5, 0x0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->a(Lcom/kamcord/android/core/KC_U;)Z

    move-result v0

    if-eqz v0, :cond_0

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->b(Lcom/kamcord/android/core/KC_U;)Ljava/io/FileOutputStream;

    move-result-object v0

    invoke-virtual {v0}, Ljava/io/FileOutputStream;->close()V

    const-string v0, "VoiceWriter"

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Finished voice recording, recorded "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v2, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v2}, Lcom/kamcord/android/core/KC_U;->c(Lcom/kamcord/android/core/KC_U;)I

    move-result v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " bytes total."

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->c(Lcom/kamcord/android/core/KC_U;)I

    move-result v0

    new-instance v1, Ljava/io/RandomAccessFile;

    iget-object v2, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    iget-object v2, v2, Lcom/kamcord/android/core/KC_U;->c:Ljava/lang/String;

    const-string v3, "rw"

    invoke-direct {v1, v2, v3}, Ljava/io/RandomAccessFile;-><init>(Ljava/lang/String;Ljava/lang/String;)V

    const-wide/16 v2, 0x4

    invoke-virtual {v1, v2, v3}, Ljava/io/RandomAccessFile;->seek(J)V

    add-int/lit8 v2, v0, 0x24

    invoke-static {v2}, Lcom/kamcord/android/core/KC_U;->a(I)[B

    move-result-object v2

    const/4 v3, 0x0

    const/4 v4, 0x4

    invoke-virtual {v1, v2, v3, v4}, Ljava/io/RandomAccessFile;->write([BII)V

    const-wide/16 v2, 0x28

    invoke-virtual {v1, v2, v3}, Ljava/io/RandomAccessFile;->seek(J)V

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->a(I)[B

    move-result-object v0

    const/4 v2, 0x0

    const/4 v3, 0x4

    invoke-virtual {v1, v0, v2, v3}, Ljava/io/RandomAccessFile;->write([BII)V

    invoke-virtual {v1}, Ljava/io/RandomAccessFile;->close()V
    :try_end_0
    .catch Ljava/io/IOException; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_U;->a:Landroid/media/AudioRecord;

    invoke-virtual {v0}, Landroid/media/AudioRecord;->stop()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_U;->a:Landroid/media/AudioRecord;

    invoke-virtual {v0}, Landroid/media/AudioRecord;->release()V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    const/4 v1, 0x0

    iput-object v1, v0, Lcom/kamcord/android/core/KC_U;->a:Landroid/media/AudioRecord;

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0, v5}, Lcom/kamcord/android/core/KC_U;->a(Lcom/kamcord/android/core/KC_U;Z)Z

    :cond_0
    return-void

    :catch_0
    move-exception v0

    const-string v1, "Something unexpected happened when stopping the voice track. This may lead to an incorrectly formatted .wav file."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/io/IOException;->printStackTrace()V

    goto :goto_0
.end method

.method private b()V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_U;->a:Landroid/media/AudioRecord;

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_U;->a:Landroid/media/AudioRecord;

    invoke-virtual {v0}, Landroid/media/AudioRecord;->release()V

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    const/4 v1, 0x0

    iput-object v1, v0, Lcom/kamcord/android/core/KC_U;->a:Landroid/media/AudioRecord;

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    const/4 v1, 0x0

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KC_U;->a(Lcom/kamcord/android/core/KC_U;Z)Z

    const-string v0, "Removing voice track."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->b(Lcom/kamcord/android/core/KC_U;)Ljava/io/FileOutputStream;

    move-result-object v0

    invoke-virtual {v0}, Ljava/io/FileOutputStream;->close()V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    new-instance v0, Ljava/io/File;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    iget-object v1, v1, Lcom/kamcord/android/core/KC_U;->c:Ljava/lang/String;

    invoke-direct {v0, v1}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0}, Ljava/io/File;->delete()Z

    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->a:Z

    return-void

    :catch_0
    move-exception v0

    goto :goto_0
.end method


# virtual methods
.method public final a(Landroid/os/Message;)V
    .locals 9

    const/4 v1, 0x1

    const/4 v8, 0x4

    const/4 v2, 0x0

    iget v0, p1, Landroid/os/Message;->what:I

    packed-switch v0, :pswitch_data_0

    :cond_0
    :goto_0
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->a(Lcom/kamcord/android/core/KC_U;)Z

    move-result v0

    if-nez v0, :cond_1

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_U;->a:Landroid/media/AudioRecord;

    invoke-virtual {v0}, Landroid/media/AudioRecord;->startRecording()V
    :try_end_0
    .catch Ljava/lang/IllegalStateException; {:try_start_0 .. :try_end_0} :catch_0

    :goto_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0, v2}, Lcom/kamcord/android/core/KC_U;->a(Lcom/kamcord/android/core/KC_U;I)I

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0, v2}, Lcom/kamcord/android/core/KC_U;->b(Lcom/kamcord/android/core/KC_U;I)I

    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    new-instance v1, Ljava/io/FileOutputStream;

    iget-object v2, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    iget-object v2, v2, Lcom/kamcord/android/core/KC_U;->c:Ljava/lang/String;

    invoke-direct {v1, v2}, Ljava/io/FileOutputStream;-><init>(Ljava/lang/String;)V

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KC_U;->a(Lcom/kamcord/android/core/KC_U;Ljava/io/FileOutputStream;)Ljava/io/FileOutputStream;

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->b(Lcom/kamcord/android/core/KC_U;)Ljava/io/FileOutputStream;

    move-result-object v0

    const/16 v1, 0x2c

    new-array v1, v1, [B

    const/4 v2, 0x4

    new-array v2, v2, [B

    fill-array-data v2, :array_0

    const/4 v3, 0x0

    const/4 v4, 0x0

    const/4 v5, 0x4

    invoke-static {v2, v3, v1, v4, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    const/4 v2, 0x0

    invoke-static {v2}, Lcom/kamcord/android/core/KC_U;->a(I)[B

    move-result-object v2

    const/4 v3, 0x0

    const/4 v4, 0x4

    const/4 v5, 0x4

    invoke-static {v2, v3, v1, v4, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    const/4 v2, 0x4

    new-array v2, v2, [B

    fill-array-data v2, :array_1

    const/4 v3, 0x0

    const/16 v4, 0x8

    const/4 v5, 0x4

    invoke-static {v2, v3, v1, v4, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    iget-object v2, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    iget v2, v2, Lcom/kamcord/android/core/KC_U;->b:I

    shl-int/lit8 v3, v2, 0x4

    div-int/lit8 v3, v3, 0x2

    const/4 v4, 0x4

    new-array v4, v4, [B

    fill-array-data v4, :array_2

    const/4 v5, 0x0

    const/16 v6, 0xc

    const/4 v7, 0x4

    invoke-static {v4, v5, v1, v6, v7}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    const/16 v4, 0x10

    invoke-static {v4}, Lcom/kamcord/android/core/KC_U;->a(I)[B

    move-result-object v4

    const/4 v5, 0x0

    const/16 v6, 0x10

    const/4 v7, 0x4

    invoke-static {v4, v5, v1, v6, v7}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    const/4 v4, 0x1

    invoke-static {v4}, Lcom/kamcord/android/core/KC_U;->a(S)[B

    move-result-object v4

    const/4 v5, 0x0

    const/16 v6, 0x14

    const/4 v7, 0x2

    invoke-static {v4, v5, v1, v6, v7}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    const/4 v4, 0x1

    invoke-static {v4}, Lcom/kamcord/android/core/KC_U;->a(S)[B

    move-result-object v4

    const/4 v5, 0x0

    const/16 v6, 0x16

    const/4 v7, 0x2

    invoke-static {v4, v5, v1, v6, v7}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    invoke-static {v2}, Lcom/kamcord/android/core/KC_U;->a(I)[B

    move-result-object v2

    const/4 v4, 0x0

    const/16 v5, 0x18

    const/4 v6, 0x4

    invoke-static {v2, v4, v1, v5, v6}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    invoke-static {v3}, Lcom/kamcord/android/core/KC_U;->a(I)[B

    move-result-object v2

    const/4 v3, 0x0

    const/16 v4, 0x1c

    const/4 v5, 0x4

    invoke-static {v2, v3, v1, v4, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    const/16 v2, 0x8

    invoke-static {v2}, Lcom/kamcord/android/core/KC_U;->a(S)[B

    move-result-object v2

    const/4 v3, 0x0

    const/16 v4, 0x20

    const/4 v5, 0x2

    invoke-static {v2, v3, v1, v4, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    const/16 v2, 0x10

    invoke-static {v2}, Lcom/kamcord/android/core/KC_U;->a(S)[B

    move-result-object v2

    const/4 v3, 0x0

    const/16 v4, 0x22

    const/4 v5, 0x2

    invoke-static {v2, v3, v1, v4, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    const/4 v2, 0x4

    new-array v2, v2, [B

    fill-array-data v2, :array_3

    const/4 v3, 0x0

    const/16 v4, 0x24

    const/4 v5, 0x4

    invoke-static {v2, v3, v1, v4, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    const/4 v2, 0x0

    invoke-static {v2}, Lcom/kamcord/android/core/KC_U;->a(I)[B

    move-result-object v2

    const/4 v3, 0x0

    const/16 v4, 0x28

    const/4 v5, 0x4

    invoke-static {v2, v3, v1, v4, v5}, Ljava/lang/System;->arraycopy(Ljava/lang/Object;ILjava/lang/Object;II)V

    const/4 v2, 0x0

    const/16 v3, 0x2c

    invoke-virtual {v0, v1, v2, v3}, Ljava/io/FileOutputStream;->write([BII)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    const/4 v1, 0x1

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KC_U;->a(Lcom/kamcord/android/core/KC_U;Z)Z
    :try_end_1
    .catch Ljava/io/FileNotFoundException; {:try_start_1 .. :try_end_1} :catch_1
    .catch Ljava/io/IOException; {:try_start_1 .. :try_end_1} :catch_2

    :cond_1
    :goto_2
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->a(Lcom/kamcord/android/core/KC_U;)Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-virtual {p0, v8}, Lcom/kamcord/android/core/KC_U$KC_a;->sendEmptyMessage(I)Z

    goto/16 :goto_0

    :catch_0
    move-exception v0

    const-string v1, "Something unexpected happened when starting to record voice!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/IllegalStateException;->printStackTrace()V

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_U$KC_a;->b()V

    goto/16 :goto_1

    :catch_1
    move-exception v0

    const-string v1, "Couldn\'t create voice track file!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/io/FileNotFoundException;->printStackTrace()V

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_U$KC_a;->b()V

    goto :goto_2

    :catch_2
    move-exception v0

    const-string v1, "Couldn\'t write voice track header!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/io/IOException;->printStackTrace()V

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_U$KC_a;->b()V

    goto :goto_2

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->d(Lcom/kamcord/android/core/KC_U;)Z

    move-result v0

    if-nez v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->a(Lcom/kamcord/android/core/KC_U;)Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KC_U;->b(Lcom/kamcord/android/core/KC_U;Z)Z

    goto/16 :goto_0

    :pswitch_2
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->d(Lcom/kamcord/android/core/KC_U;)Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->a(Lcom/kamcord/android/core/KC_U;)Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0, v2}, Lcom/kamcord/android/core/KC_U;->b(Lcom/kamcord/android/core/KC_U;Z)Z

    goto/16 :goto_0

    :pswitch_3
    invoke-direct {p0}, Lcom/kamcord/android/core/KC_U$KC_a;->a()V

    goto/16 :goto_0

    :pswitch_4
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->e(Lcom/kamcord/android/core/KC_U;)I

    move-result v0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v1}, Lcom/kamcord/android/core/KC_U;->f(Lcom/kamcord/android/core/KC_U;)I

    move-result v1

    if-lt v0, v1, :cond_2

    const-string v0, "Voice audio buffer is full!"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    :cond_2
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    iget-object v0, v0, Lcom/kamcord/android/core/KC_U;->a:Landroid/media/AudioRecord;

    iget-object v1, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v1}, Lcom/kamcord/android/core/KC_U;->g(Lcom/kamcord/android/core/KC_U;)Ljava/nio/ByteBuffer;

    move-result-object v1

    invoke-virtual {v1}, Ljava/nio/ByteBuffer;->array()[B

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v2}, Lcom/kamcord/android/core/KC_U;->e(Lcom/kamcord/android/core/KC_U;)I

    move-result v2

    iget-object v3, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v3}, Lcom/kamcord/android/core/KC_U;->h(Lcom/kamcord/android/core/KC_U;)I

    move-result v3

    invoke-virtual {v0, v1, v2, v3}, Landroid/media/AudioRecord;->read([BII)I

    move-result v0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v1}, Lcom/kamcord/android/core/KC_U;->d(Lcom/kamcord/android/core/KC_U;)Z

    move-result v1

    if-nez v1, :cond_3

    if-ltz v0, :cond_4

    iget-object v1, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v1, v0}, Lcom/kamcord/android/core/KC_U;->c(Lcom/kamcord/android/core/KC_U;I)I

    iget-object v1, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v1, v0}, Lcom/kamcord/android/core/KC_U;->d(Lcom/kamcord/android/core/KC_U;I)I

    :cond_3
    :goto_3
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->a(Lcom/kamcord/android/core/KC_U;)Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-virtual {p0, v8}, Lcom/kamcord/android/core/KC_U$KC_a;->sendEmptyMessage(I)Z

    goto/16 :goto_0

    :cond_4
    const-string v1, "VoiceWriter"

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "AudioRecord.read(...) returned "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v1, v0}, Lcom/kamcord/android/Kamcord$KC_a;->b(Ljava/lang/String;Ljava/lang/String;)I

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_U$KC_a;->a()V

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_U$KC_a;->b()V

    goto :goto_3

    :pswitch_5
    :try_start_2
    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v0}, Lcom/kamcord/android/core/KC_U;->b(Lcom/kamcord/android/core/KC_U;)Ljava/io/FileOutputStream;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v1}, Lcom/kamcord/android/core/KC_U;->g(Lcom/kamcord/android/core/KC_U;)Ljava/nio/ByteBuffer;

    move-result-object v1

    invoke-virtual {v1}, Ljava/nio/ByteBuffer;->array()[B

    move-result-object v1

    const/4 v2, 0x0

    iget-object v3, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    invoke-static {v3}, Lcom/kamcord/android/core/KC_U;->e(Lcom/kamcord/android/core/KC_U;)I

    move-result v3

    invoke-virtual {v0, v1, v2, v3}, Ljava/io/FileOutputStream;->write([BII)V

    iget-object v0, p0, Lcom/kamcord/android/core/KC_U$KC_a;->b:Lcom/kamcord/android/core/KC_U;

    const/4 v1, 0x0

    invoke-static {v0, v1}, Lcom/kamcord/android/core/KC_U;->b(Lcom/kamcord/android/core/KC_U;I)I
    :try_end_2
    .catch Ljava/io/IOException; {:try_start_2 .. :try_end_2} :catch_3

    goto/16 :goto_0

    :catch_3
    move-exception v0

    const-string v1, "IOException while writing voice samples. Did you run out of room on your external storage?"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/io/IOException;->printStackTrace()V

    invoke-direct {p0}, Lcom/kamcord/android/core/KC_U$KC_a;->b()V

    goto/16 :goto_0

    :pswitch_data_0
    .packed-switch 0x0
        :pswitch_0
        :pswitch_3
        :pswitch_1
        :pswitch_2
        :pswitch_4
        :pswitch_5
    .end packed-switch

    :array_0
    .array-data 1
        0x52t
        0x49t
        0x46t
        0x46t
    .end array-data

    :array_1
    .array-data 1
        0x57t
        0x41t
        0x56t
        0x45t
    .end array-data

    :array_2
    .array-data 1
        0x66t
        0x6dt
        0x74t
        0x20t
    .end array-data

    :array_3
    .array-data 1
        0x64t
        0x61t
        0x74t
        0x61t
    .end array-data
.end method

.method public final b(Landroid/os/Message;)V
    .locals 0

    return-void
.end method
