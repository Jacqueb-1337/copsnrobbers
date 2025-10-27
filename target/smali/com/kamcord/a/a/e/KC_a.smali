.class public Lcom/kamcord/a/a/e/KC_a;
.super Ljava/lang/Object;

# interfaces
.implements Lcom/kamcord/a/a/e/KC_c;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/a/a/e/KC_a$1;,
        Lcom/kamcord/a/a/e/KC_a$KC_a;
    }
.end annotation


# instance fields
.field private a:Lcom/kamcord/a/a/d/KC_a;

.field private b:Lcom/kamcord/a/a/a/a/KC_b;


# direct methods
.method public constructor <init>(Lcom/kamcord/a/a/a/a/KC_b;Lcom/kamcord/a/a/d/KC_a;)V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-object p1, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    iput-object p2, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    return-void
.end method

.method private a(Lcom/kamcord/a/a/d/KC_c;)V
    .locals 3

    sget-object v0, Lcom/kamcord/a/a/e/KC_a$1;->a:[I

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v1}, Lcom/kamcord/a/a/d/KC_a;->d()Lcom/kamcord/a/a/d/KC_i;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/a/a/d/KC_i;->ordinal()I

    move-result v1

    aget v0, v0, v1

    packed-switch v0, :pswitch_data_0

    :cond_0
    :goto_0
    return-void

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    const-string v1, "using Http Header signature"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    new-instance v0, Lcom/kamcord/a/a/c/KC_c;

    invoke-direct {v0}, Lcom/kamcord/a/a/c/KC_c;-><init>()V

    invoke-virtual {v0, p1}, Lcom/kamcord/a/a/c/KC_c;->a(Lcom/kamcord/a/a/d/KC_c;)Ljava/lang/String;

    move-result-object v0

    const-string v1, "Authorization"

    invoke-virtual {p1, v1, v0}, Lcom/kamcord/a/a/d/KC_c;->b(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    const-string v1, "using Querystring signature"

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_c;->a()Ljava/util/Map;

    move-result-object v0

    invoke-interface {v0}, Ljava/util/Map;->entrySet()Ljava/util/Set;

    move-result-object v0

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v2

    :goto_1
    invoke-interface {v2}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v2}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Map$Entry;

    invoke-interface {v0}, Ljava/util/Map$Entry;->getKey()Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Ljava/lang/String;

    invoke-interface {v0}, Ljava/util/Map$Entry;->getValue()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/lang/String;

    invoke-virtual {p1, v1, v0}, Lcom/kamcord/a/a/d/KC_c;->d(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_1

    nop

    :pswitch_data_0
    .packed-switch 0x1
        :pswitch_0
        :pswitch_1
    .end packed-switch
.end method

.method private a(Lcom/kamcord/a/a/d/KC_c;Lcom/kamcord/a/a/d/KC_j;)V
    .locals 6

    const-string v0, "oauth_timestamp"

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    new-instance v1, Lcom/kamcord/a/a/f/KC_e;

    invoke-direct {v1}, Lcom/kamcord/a/a/f/KC_e;-><init>()V

    invoke-virtual {v1}, Lcom/kamcord/a/a/f/KC_e;->a()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p1, v0, v1}, Lcom/kamcord/a/a/d/KC_c;->a(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "oauth_nonce"

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    new-instance v1, Lcom/kamcord/a/a/f/KC_e;

    invoke-direct {v1}, Lcom/kamcord/a/a/f/KC_e;-><init>()V

    invoke-virtual {v1}, Lcom/kamcord/a/a/f/KC_e;->b()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p1, v0, v1}, Lcom/kamcord/a/a/d/KC_c;->a(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "oauth_consumer_key"

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v1}, Lcom/kamcord/a/a/d/KC_a;->a()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p1, v0, v1}, Lcom/kamcord/a/a/d/KC_c;->a(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "oauth_signature_method"

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    new-instance v1, Lcom/kamcord/a/a/f/KC_d;

    invoke-direct {v1}, Lcom/kamcord/a/a/f/KC_d;-><init>()V

    invoke-virtual {v1}, Lcom/kamcord/a/a/f/KC_d;->a()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p1, v0, v1}, Lcom/kamcord/a/a/d/KC_c;->a(Ljava/lang/String;Ljava/lang/String;)V

    const-string v0, "oauth_version"

    const-string v1, "1.0"

    invoke-virtual {p1, v0, v1}, Lcom/kamcord/a/a/d/KC_c;->a(Ljava/lang/String;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_a;->f()Z

    move-result v0

    if-eqz v0, :cond_0

    const-string v0, "scope"

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v1}, Lcom/kamcord/a/a/d/KC_a;->e()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p1, v0, v1}, Lcom/kamcord/a/a/d/KC_c;->a(Ljava/lang/String;Ljava/lang/String;)V

    :cond_0
    const-string v0, "oauth_signature"

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    const-string v2, "generating signature..."

    invoke-virtual {v1, v2}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "using base64 encoder: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {}, Lcom/kamcord/a/a/f/KC_a;->a()Lcom/kamcord/a/a/f/KC_a;

    move-result-object v3

    invoke-virtual {v3}, Lcom/kamcord/a/a/f/KC_a;->b()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    new-instance v1, Lcom/kamcord/a/a/c/KC_b;

    invoke-direct {v1}, Lcom/kamcord/a/a/c/KC_b;-><init>()V

    invoke-virtual {v1, p1}, Lcom/kamcord/a/a/c/KC_b;->a(Lcom/kamcord/a/a/d/KC_c;)Ljava/lang/String;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    new-instance v2, Lcom/kamcord/a/a/f/KC_d;

    invoke-direct {v2}, Lcom/kamcord/a/a/f/KC_d;-><init>()V

    iget-object v3, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v3}, Lcom/kamcord/a/a/d/KC_a;->b()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {p2}, Lcom/kamcord/a/a/d/KC_j;->b()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v2, v1, v3, v4}, Lcom/kamcord/a/a/f/KC_d;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    iget-object v3, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v4, Ljava/lang/StringBuilder;

    const-string v5, "base string is: "

    invoke-direct {v4, v5}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v4, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v3, v1}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "signature is: "

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v3, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v1, v3}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    invoke-virtual {p1, v0, v2}, Lcom/kamcord/a/a/d/KC_c;->a(Ljava/lang/String;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "appended additional OAuth parameters: "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_c;->a()Ljava/util/Map;

    move-result-object v2

    invoke-static {v2}, La/a/a/c/KC_a;->a(Ljava/util/Map;)Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    return-void
.end method


# virtual methods
.method public final a(Lcom/kamcord/a/a/d/KC_j;Lcom/kamcord/a/a/d/KC_l;)Lcom/kamcord/a/a/d/KC_j;
    .locals 5

    sget-object v0, Ljava/util/concurrent/TimeUnit;->SECONDS:Ljava/util/concurrent/TimeUnit;

    new-instance v1, Lcom/kamcord/a/a/e/KC_a$KC_a;

    const/4 v2, 0x2

    invoke-direct {v1, v2, v0}, Lcom/kamcord/a/a/e/KC_a$KC_a;-><init>(ILjava/util/concurrent/TimeUnit;)V

    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "obtaining access token from "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v3, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    invoke-virtual {v3}, Lcom/kamcord/a/a/a/a/KC_b;->b()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v2}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    new-instance v0, Lcom/kamcord/a/a/d/KC_c;

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    sget-object v2, Lcom/kamcord/a/a/d/KC_k;->b:Lcom/kamcord/a/a/d/KC_k;

    iget-object v3, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    invoke-virtual {v3}, Lcom/kamcord/a/a/a/a/KC_b;->b()Ljava/lang/String;

    move-result-object v3

    invoke-direct {v0, v2, v3}, Lcom/kamcord/a/a/d/KC_c;-><init>(Lcom/kamcord/a/a/d/KC_k;Ljava/lang/String;)V

    const-string v2, "oauth_token"

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_j;->a()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v0, v2, v3}, Lcom/kamcord/a/a/d/KC_c;->a(Ljava/lang/String;Ljava/lang/String;)V

    const-string v2, "oauth_verifier"

    invoke-virtual {p2}, Lcom/kamcord/a/a/d/KC_l;->a()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v0, v2, v3}, Lcom/kamcord/a/a/d/KC_c;->a(Ljava/lang/String;Ljava/lang/String;)V

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "setting token to: "

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v3, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, " and verifier to: "

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3, p2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    invoke-direct {p0, v0, p1}, Lcom/kamcord/a/a/e/KC_a;->a(Lcom/kamcord/a/a/d/KC_c;Lcom/kamcord/a/a/d/KC_j;)V

    invoke-direct {p0, v0}, Lcom/kamcord/a/a/e/KC_a;->a(Lcom/kamcord/a/a/d/KC_c;)V

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_c;->a(Lcom/kamcord/a/a/d/KC_g;)Lcom/kamcord/a/a/d/KC_h;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    new-instance v1, Lcom/kamcord/a/a/c/KC_e;

    invoke-direct {v1}, Lcom/kamcord/a/a/c/KC_e;-><init>()V

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_h;->b()Ljava/lang/String;

    move-result-object v0

    invoke-interface {v1, v0}, Lcom/kamcord/a/a/c/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    return-object v0
.end method

.method public final a(Lcom/kamcord/a/a/d/KC_l;)Lcom/kamcord/a/a/d/KC_j;
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
    .locals 3

    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "signing request: "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {p2}, Lcom/kamcord/a/a/d/KC_c;->c()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_j;->c()Z

    move-result v0

    if-nez v0, :cond_0

    const-string v0, "oauth_token"

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_j;->a()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {p2, v0, v1}, Lcom/kamcord/a/a/d/KC_c;->a(Ljava/lang/String;Ljava/lang/String;)V

    :cond_0
    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "setting token to: "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    invoke-direct {p0, p2, p1}, Lcom/kamcord/a/a/e/KC_a;->a(Lcom/kamcord/a/a/d/KC_c;Lcom/kamcord/a/a/d/KC_j;)V

    invoke-direct {p0, p2}, Lcom/kamcord/a/a/e/KC_a;->a(Lcom/kamcord/a/a/d/KC_c;)V

    return-void
.end method

.method public b()Ljava/lang/String;
    .locals 1

    const-string v0, ""

    return-object v0
.end method

.method public final b(Lcom/kamcord/a/a/d/KC_j;)Ljava/lang/String;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    invoke-virtual {v0, p1}, Lcom/kamcord/a/a/a/a/KC_b;->a(Lcom/kamcord/a/a/d/KC_j;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method public c()Ljava/lang/String;
    .locals 1

    const-string v0, ""

    return-object v0
.end method

.method public final d()Lcom/kamcord/a/a/d/KC_j;
    .locals 5

    sget-object v0, Ljava/util/concurrent/TimeUnit;->SECONDS:Ljava/util/concurrent/TimeUnit;

    new-instance v1, Lcom/kamcord/a/a/e/KC_a$KC_a;

    const/4 v2, 0x2

    invoke-direct {v1, v2, v0}, Lcom/kamcord/a/a/e/KC_a$KC_a;-><init>(ILjava/util/concurrent/TimeUnit;)V

    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "obtaining request token from "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v3, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    invoke-virtual {v3}, Lcom/kamcord/a/a/a/a/KC_b;->a()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v2}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    new-instance v0, Lcom/kamcord/a/a/d/KC_c;

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    sget-object v2, Lcom/kamcord/a/a/d/KC_k;->b:Lcom/kamcord/a/a/d/KC_k;

    iget-object v3, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    invoke-virtual {v3}, Lcom/kamcord/a/a/a/a/KC_b;->a()Ljava/lang/String;

    move-result-object v3

    invoke-direct {v0, v2, v3}, Lcom/kamcord/a/a/d/KC_c;-><init>(Lcom/kamcord/a/a/d/KC_k;Ljava/lang/String;)V

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "setting oauth_callback to "

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v4, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v4}, Lcom/kamcord/a/a/d/KC_a;->c()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    const-string v2, "oauth_callback"

    iget-object v3, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    invoke-virtual {v3}, Lcom/kamcord/a/a/d/KC_a;->c()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v0, v2, v3}, Lcom/kamcord/a/a/d/KC_c;->a(Ljava/lang/String;Ljava/lang/String;)V

    sget-object v2, Lcom/kamcord/a/a/d/KC_b;->a:Lcom/kamcord/a/a/d/KC_j;

    invoke-direct {p0, v0, v2}, Lcom/kamcord/a/a/e/KC_a;->a(Lcom/kamcord/a/a/d/KC_c;Lcom/kamcord/a/a/d/KC_j;)V

    invoke-direct {p0, v0}, Lcom/kamcord/a/a/e/KC_a;->a(Lcom/kamcord/a/a/d/KC_c;)V

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    const-string v3, "sending request..."

    invoke-virtual {v2, v3}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    invoke-virtual {v0, v1}, Lcom/kamcord/a/a/d/KC_c;->a(Lcom/kamcord/a/a/d/KC_g;)Lcom/kamcord/a/a/d/KC_h;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_h;->b()Ljava/lang/String;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "response status code: "

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_h;->c()I

    move-result v0

    invoke-virtual {v3, v0}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v2, v0}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->a:Lcom/kamcord/a/a/d/KC_a;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "response body: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v2}, Lcom/kamcord/a/a/d/KC_a;->a(Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/a/a/e/KC_a;->b:Lcom/kamcord/a/a/a/a/KC_b;

    new-instance v0, Lcom/kamcord/a/a/c/KC_e;

    invoke-direct {v0}, Lcom/kamcord/a/a/c/KC_e;-><init>()V

    invoke-interface {v0, v1}, Lcom/kamcord/a/a/c/KC_a;->a(Ljava/lang/String;)Lcom/kamcord/a/a/d/KC_j;

    move-result-object v0

    return-object v0
.end method

.method public final e()Ljava/lang/String;
    .locals 1

    const-string v0, "1.0"

    return-object v0
.end method
