.class public Lcom/kamcord/a/a/a/a/KC_e;
.super Lcom/kamcord/a/a/a/a/KC_c;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/a/a/a/a/KC_e$2;,
        Lcom/kamcord/a/a/a/a/KC_e$KC_a;
    }
.end annotation


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/a/a/a/a/KC_c;-><init>()V

    return-void
.end method


# virtual methods
.method public final a()Lcom/kamcord/a/a/c/KC_a;
    .locals 1

    new-instance v0, Lcom/kamcord/a/a/a/a/KC_e$1;

    invoke-direct {v0, p0}, Lcom/kamcord/a/a/a/a/KC_e$1;-><init>(Lcom/kamcord/a/a/a/a/KC_e;)V

    return-object v0
.end method

.method public final a(Lcom/kamcord/a/a/d/KC_a;)Lcom/kamcord/a/a/e/KC_c;
    .locals 1

    new-instance v0, Lcom/kamcord/a/a/a/a/KC_e$KC_a;

    invoke-direct {v0, p0, p0, p1}, Lcom/kamcord/a/a/a/a/KC_e$KC_a;-><init>(Lcom/kamcord/a/a/a/a/KC_e;Lcom/kamcord/a/a/a/a/KC_c;Lcom/kamcord/a/a/d/KC_a;)V

    return-object v0
.end method

.method public final b()Lcom/kamcord/a/a/d/KC_k;
    .locals 1

    sget-object v0, Lcom/kamcord/a/a/d/KC_k;->b:Lcom/kamcord/a/a/d/KC_k;

    return-object v0
.end method

.method public final b(Lcom/kamcord/a/a/d/KC_a;)Ljava/lang/String;
    .locals 6

    const/4 v5, 0x2

    const/4 v4, 0x1

    const/4 v3, 0x0

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_a;->f()Z

    move-result v0

    if-eqz v0, :cond_0

    const-string v0, "https://accounts.google.com/o/oauth2/auth?response_type=code&client_id=%s&redirect_uri=%s&offline=true&scope=%s"

    const/4 v1, 0x3

    new-array v1, v1, [Ljava/lang/Object;

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_a;->a()Ljava/lang/String;

    move-result-object v2

    aput-object v2, v1, v3

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_a;->c()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/a/a/g/KC_a;->a(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    aput-object v2, v1, v4

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_a;->e()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/a/a/g/KC_a;->a(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    aput-object v2, v1, v5

    invoke-static {v0, v1}, Ljava/lang/String;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v0

    :goto_0
    return-object v0

    :cond_0
    const-string v0, "https://accounts.google.com/o/oauth2/auth?response_type=code&client_id=%s&redirect_uri=%s&offline=true"

    new-array v1, v5, [Ljava/lang/Object;

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_a;->a()Ljava/lang/String;

    move-result-object v2

    aput-object v2, v1, v3

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_a;->c()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/a/a/g/KC_a;->a(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    aput-object v2, v1, v4

    invoke-static {v0, v1}, Ljava/lang/String;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v0

    goto :goto_0
.end method

.method public final c()Ljava/lang/String;
    .locals 1

    const-string v0, "https://accounts.google.com/o/oauth2/token"

    return-object v0
.end method
