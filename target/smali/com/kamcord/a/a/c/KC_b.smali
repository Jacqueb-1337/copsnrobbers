.class public Lcom/kamcord/a/a/c/KC_b;
.super Ljava/lang/Object;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public a(Lcom/kamcord/a/a/d/KC_c;)Ljava/lang/String;
    .locals 6

    const-string v0, "Cannot extract base string from null object"

    invoke-static {p1, v0}, Lcom/kamcord/a/a/g/KC_b;->a(Ljava/lang/Object;Ljava/lang/String;)V

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_c;->a()Ljava/util/Map;

    move-result-object v0

    if-eqz v0, :cond_0

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_c;->a()Ljava/util/Map;

    move-result-object v0

    invoke-interface {v0}, Ljava/util/Map;->size()I

    move-result v0

    if-gtz v0, :cond_1

    :cond_0
    new-instance v0, Lcom/kamcord/a/a/b/KC_c;

    invoke-direct {v0, p1}, Lcom/kamcord/a/a/b/KC_c;-><init>(Lcom/kamcord/a/a/d/KC_c;)V

    throw v0

    :cond_1
    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_c;->h()Lcom/kamcord/a/a/d/KC_k;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/a/a/d/KC_k;->name()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/a/a/g/KC_a;->a(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_c;->g()Ljava/lang/String;

    move-result-object v1

    invoke-static {v1}, Lcom/kamcord/a/a/g/KC_a;->a(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    new-instance v2, Lcom/kamcord/a/a/d/KC_e;

    invoke-direct {v2}, Lcom/kamcord/a/a/d/KC_e;-><init>()V

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_c;->d()Lcom/kamcord/a/a/d/KC_e;

    move-result-object v3

    invoke-virtual {v2, v3}, Lcom/kamcord/a/a/d/KC_e;->a(Lcom/kamcord/a/a/d/KC_e;)V

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_c;->e()Lcom/kamcord/a/a/d/KC_e;

    move-result-object v3

    invoke-virtual {v2, v3}, Lcom/kamcord/a/a/d/KC_e;->a(Lcom/kamcord/a/a/d/KC_e;)V

    new-instance v3, Lcom/kamcord/a/a/d/KC_e;

    invoke-virtual {p1}, Lcom/kamcord/a/a/d/KC_c;->a()Ljava/util/Map;

    move-result-object v4

    invoke-direct {v3, v4}, Lcom/kamcord/a/a/d/KC_e;-><init>(Ljava/util/Map;)V

    invoke-virtual {v2, v3}, Lcom/kamcord/a/a/d/KC_e;->a(Lcom/kamcord/a/a/d/KC_e;)V

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_e;->b()Lcom/kamcord/a/a/d/KC_e;

    move-result-object v2

    invoke-virtual {v2}, Lcom/kamcord/a/a/d/KC_e;->a()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/a/a/g/KC_a;->a(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v2

    const-string v3, "%s&%s&%s"

    const/4 v4, 0x3

    new-array v4, v4, [Ljava/lang/Object;

    const/4 v5, 0x0

    aput-object v0, v4, v5

    const/4 v0, 0x1

    aput-object v1, v4, v0

    const/4 v0, 0x2

    aput-object v2, v4, v0

    invoke-static {v3, v4}, Ljava/lang/String;->format(Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method
