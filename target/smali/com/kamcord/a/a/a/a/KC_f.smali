.class public Lcom/kamcord/a/a/a/a/KC_f;
.super Lcom/kamcord/a/a/a/a/KC_b;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/a/a/a/a/KC_f$KC_a;
    }
.end annotation


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/a/a/a/a/KC_b;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(Lcom/kamcord/a/a/d/KC_a;)Lcom/kamcord/a/a/e/KC_c;
    .locals 1

    new-instance v0, Lcom/kamcord/a/a/a/a/KC_f$KC_a;

    invoke-direct {v0, p0, p0, p1}, Lcom/kamcord/a/a/a/a/KC_f$KC_a;-><init>(Lcom/kamcord/a/a/a/a/KC_f;Lcom/kamcord/a/a/a/a/KC_b;Lcom/kamcord/a/a/d/KC_a;)V

    return-object v0
.end method

.method public final a()Ljava/lang/String;
    .locals 1

    const-string v0, "https://api.twitter.com/oauth/request_token"

    return-object v0
.end method

.method public final a(Lcom/kamcord/a/a/d/KC_j;)Ljava/lang/String;
    .locals 4

    :try_start_0
    const-string v0, "https://api.twitter.com/oauth/authorize?oauth_token=%s"

    const/4 v1, 0x1

    new-array v1, v1, [Ljava/lang/Object;

    const/4 v2, 0x0

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_j;->a()Ljava/lang/String;

    move-result-object v3

    aput-object v3, v1, v2

    invoke-static {v0, v1}, Ljava/lang/String;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/String;
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    move-result-object v0

    :goto_0
    return-object v0

    :catch_0
    move-exception v0

    sget-object v1, Ljava/lang/System;->out:Ljava/io/PrintStream;

    invoke-virtual {v0}, Ljava/lang/Exception;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v1, v0}, Ljava/io/PrintStream;->println(Ljava/lang/String;)V

    const/4 v0, 0x0

    goto :goto_0
.end method

.method public final b()Ljava/lang/String;
    .locals 1

    const-string v0, "https://api.twitter.com/oauth/access_token"

    return-object v0
.end method
