.class public Lcom/kamcord/a/a/e/KC_b;
.super Ljava/lang/Object;

# interfaces
.implements Lcom/kamcord/a/a/e/KC_c;


# instance fields
.field private final a:Lcom/kamcord/a/a/a/a/KC_c;

.field private final b:Lcom/kamcord/a/a/d/KC_a;


# direct methods
.method public constructor <init>(Lcom/kamcord/a/a/a/a/KC_c;Lcom/kamcord/a/a/d/KC_a;)V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-object p1, p0, Lcom/kamcord/a/a/e/KC_b;->a:Lcom/kamcord/a/a/a/a/KC_c;

    iput-object p2, p0, Lcom/kamcord/a/a/e/KC_b;->b:Lcom/kamcord/a/a/d/KC_a;

    return-void
.end method


# virtual methods
.method public a(Lcom/kamcord/a/a/d/KC_j;Lcom/kamcord/a/a/d/KC_l;)Lcom/kamcord/a/a/d/KC_j;
    .locals 3

    new-instance v0, Lcom/kamcord/a/a/d/KC_c;

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_b;->a:Lcom/kamcord/a/a/a/a/KC_c;

    invoke-virtual {v1}, Lcom/kamcord/a/a/a/a/KC_c;->b()Lcom/kamcord/a/a/d/KC_k;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_b;->a:Lcom/kamcord/a/a/a/a/KC_c;

    invoke-virtual {v2}, Lcom/kamcord/a/a/a/a/KC_c;->c()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;-><init>(Lcom/kamcord/a/a/d/KC_k;Ljava/lang/String;)V

    const-string v1, "client_id"

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_b;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->a()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "client_secret"

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_b;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->b()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "code"

    invoke-virtual {p2}, Lcom/kamcord/a/a/d/KC_l;->a()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    const-string v1, "redirect_uri"

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_b;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->c()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_b;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v1}, Lcom/kamcord/a/a/d/KC_a;->f()Z

    move-result v1

    if-eqz v1, :cond_0

    const-string v1, "scope"

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_b;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_a;->e()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    :cond_0
    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_c;->b()Lcom/kamcord/a/a/d/KC_h;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_b;->a:Lcom/kamcord/a/a/a/a/KC_c;

    invoke-virtual {v1}, Lcom/kamcord/a/a/a/a/KC_c;->a()Lcom/kamcord/a/a/c/KC_a;

    move-result-object v1

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_h;->b()Ljava/lang/String;

    move-result-object v0

    invoke-interface {v1, v0}, Lcom/kamcord/a/a/c/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    return-object v0
.end method

.method public a(Lcom/kamcord/a/a/d/KC_l;)Lcom/kamcord/a/a/d/KC_j;
    .locals 1

    const/4 v0, 0x0

    return-object v0
.end method

.method public a(Lcom/kamcord/a/a/d/KC_h;)Ljava/lang/String;
    .locals 1

    const-string v0, ""

    return-object v0
.end method

.method public a(Lcom/kamcord/a/a/d/KC_j;)Ljava/util/HashMap;
    .locals 1
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Lcom/kamcord/a/a/d/KC_j;",
            ")",
            "Ljava/util/HashMap",
            "<",
            "Ljava/lang/String;",
            "Ljava/lang/String;",
            ">;"
        }
    .end annotation

    const/4 v0, 0x0

    return-object v0
.end method

.method public a()V
    .locals 0

    return-void
.end method

.method public final a(Lcom/kamcord/a/a/d/KC_j;Lcom/kamcord/a/a/d/KC_c;)V
    .locals 2

    const-string v0, "access_token"

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_j;->a()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p2, v0, v1}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    return-void
.end method

.method public b()Ljava/lang/String;
    .locals 1

    const-string v0, ""

    return-object v0
.end method

.method public final b(Lcom/kamcord/a/a/d/KC_j;)Ljava/lang/String;
    .locals 2

    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_b;->a:Lcom/kamcord/a/a/a/a/KC_c;

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_b;->b:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/a/a/KC_c;->b(Lcom/kamcord/a/a/d/KC_a;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method public c()Ljava/lang/String;
    .locals 1

    const-string v0, ""

    return-object v0
.end method

.method public final d()Lcom/kamcord/a/a/d/KC_j;
    .locals 2

    new-instance v0, Ljava/lang/UnsupportedOperationException;

    const-string v1, "Unsupported operation, please use \'getAuthorizationUrl\' and redirect your users there"

    invoke-direct {v0, v1}, Ljava/lang/UnsupportedOperationException;-><init>(Ljava/lang/String;)V

    throw v0
.end method

.method public final e()Ljava/lang/String;
    .locals 1

    const-string v0, "2.0"

    return-object v0
.end method
