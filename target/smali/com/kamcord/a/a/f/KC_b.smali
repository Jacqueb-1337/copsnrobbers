.class public final Lcom/kamcord/a/a/f/KC_b;
.super Lcom/kamcord/a/a/f/KC_a;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/a/a/f/KC_a;-><init>()V

    return-void
.end method

.method public static c()Z
    .locals 1

    :try_start_0
    const-string v0, "b.a.a.a.a.KC_a"

    invoke-static {v0}, Ljava/lang/Class;->forName(Ljava/lang/String;)Ljava/lang/Class;
    :try_end_0
    .catch Ljava/lang/ClassNotFoundException; {:try_start_0 .. :try_end_0} :catch_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :catch_0
    move-exception v0

    const/4 v0, 0x0

    goto :goto_0
.end method


# virtual methods
.method public final a([B)Ljava/lang/String;
    .locals 3

    :try_start_0
    new-instance v0, Ljava/lang/String;

    invoke-static {p1}, Lb/a/a/a/a/KC_a;->a([B)[B

    move-result-object v1

    const-string v2, "UTF-8"

    invoke-direct {v0, v1, v2}, Ljava/lang/String;-><init>([BLjava/lang/String;)V
    :try_end_0
    .catch Ljava/io/UnsupportedEncodingException; {:try_start_0 .. :try_end_0} :catch_0

    return-object v0

    :catch_0
    move-exception v0

    new-instance v1, Lcom/kamcord/a/a/b/KC_d;

    const-string v2, "Can\'t perform base64 encoding"

    invoke-direct {v1, v2, v0}, Lcom/kamcord/a/a/b/KC_d;-><init>(Ljava/lang/String;Ljava/lang/Exception;)V

    throw v1
.end method

.method public final b()Ljava/lang/String;
    .locals 1

    const-string v0, "CommonsCodec"

    return-object v0
.end method
