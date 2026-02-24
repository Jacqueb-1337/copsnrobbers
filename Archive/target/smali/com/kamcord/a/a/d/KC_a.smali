.class public final Lcom/kamcord/a/a/d/KC_a;
.super Ljava/lang/Object;


# instance fields
.field private final a:Ljava/lang/String;

.field private final b:Ljava/lang/String;

.field private final c:Ljava/lang/String;

.field private final d:Lcom/kamcord/a/a/d/KC_i;

.field private final e:Ljava/lang/String;

.field private final f:Ljava/io/OutputStream;


# direct methods
.method public constructor <init>(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Lcom/kamcord/a/a/d/KC_i;Ljava/lang/String;Ljava/io/OutputStream;)V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-object p1, p0, Lcom/kamcord/a/a/d/KC_a;->a:Ljava/lang/String;

    iput-object p2, p0, Lcom/kamcord/a/a/d/KC_a;->b:Ljava/lang/String;

    iput-object p3, p0, Lcom/kamcord/a/a/d/KC_a;->c:Ljava/lang/String;

    iput-object p4, p0, Lcom/kamcord/a/a/d/KC_a;->d:Lcom/kamcord/a/a/d/KC_i;

    iput-object p5, p0, Lcom/kamcord/a/a/d/KC_a;->e:Ljava/lang/String;

    iput-object p6, p0, Lcom/kamcord/a/a/d/KC_a;->f:Ljava/io/OutputStream;

    return-void
.end method


# virtual methods
.method public final a()Ljava/lang/String;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_a;->a:Ljava/lang/String;

    return-object v0
.end method

.method public final a(Ljava/lang/String;)V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_a;->f:Ljava/io/OutputStream;

    if-eqz v0, :cond_0

    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "\n"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/a/a/d/KC_a;->f:Ljava/io/OutputStream;

    const-string v2, "UTF8"

    invoke-virtual {v0, v2}, Ljava/lang/String;->getBytes(Ljava/lang/String;)[B

    move-result-object v0

    invoke-virtual {v1, v0}, Ljava/io/OutputStream;->write([B)V
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    :cond_0
    return-void

    :catch_0
    move-exception v0

    new-instance v1, Ljava/lang/RuntimeException;

    const-string v2, "there were problems while writting to the debug stream"

    invoke-direct {v1, v2, v0}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/String;Ljava/lang/Throwable;)V

    throw v1
.end method

.method public final b()Ljava/lang/String;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_a;->b:Ljava/lang/String;

    return-object v0
.end method

.method public final c()Ljava/lang/String;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_a;->c:Ljava/lang/String;

    return-object v0
.end method

.method public final d()Lcom/kamcord/a/a/d/KC_i;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_a;->d:Lcom/kamcord/a/a/d/KC_i;

    return-object v0
.end method

.method public final e()Ljava/lang/String;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_a;->e:Ljava/lang/String;

    return-object v0
.end method

.method public final f()Z
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/d/KC_a;->e:Ljava/lang/String;

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method
