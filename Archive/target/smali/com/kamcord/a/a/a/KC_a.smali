.class public final Lcom/kamcord/a/a/a/KC_a;
.super Ljava/lang/Object;


# instance fields
.field private a:Ljava/lang/String;

.field private b:Ljava/lang/String;

.field private c:Ljava/lang/String;

.field private d:Lcom/kamcord/a/a/a/a/KC_a;

.field private e:Ljava/lang/String;

.field private f:Lcom/kamcord/a/a/d/KC_i;

.field private g:Ljava/io/OutputStream;


# direct methods
.method public constructor <init>()V
    .locals 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    const-string v0, "oob"

    iput-object v0, p0, Lcom/kamcord/a/a/a/KC_a;->c:Ljava/lang/String;

    sget-object v0, Lcom/kamcord/a/a/d/KC_i;->a:Lcom/kamcord/a/a/d/KC_i;

    iput-object v0, p0, Lcom/kamcord/a/a/a/KC_a;->f:Lcom/kamcord/a/a/d/KC_i;

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/a/a/a/KC_a;->g:Ljava/io/OutputStream;

    return-void
.end method

.method private static b(Ljava/lang/Class;)Lcom/kamcord/a/a/a/a/KC_a;
    .locals 3
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/lang/Class",
            "<+",
            "Lcom/kamcord/a/a/a/a/KC_a;",
            ">;)",
            "Lcom/kamcord/a/a/a/a/KC_a;"
        }
    .end annotation

    const-string v0, "Api class cannot be null"

    invoke-static {p0, v0}, Lcom/kamcord/a/a/g/KC_b;->a(Ljava/lang/Object;Ljava/lang/String;)V

    :try_start_0
    invoke-virtual {p0}, Ljava/lang/Class;->newInstance()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/a/a/a/a/KC_a;
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    return-object v0

    :catch_0
    move-exception v0

    new-instance v1, Lcom/kamcord/a/a/b/KC_b;

    const-string v2, "Error while creating the Api object"

    invoke-direct {v1, v2, v0}, Lcom/kamcord/a/a/b/KC_b;-><init>(Ljava/lang/String;Ljava/lang/Exception;)V

    throw v1
.end method


# virtual methods
.method public final a(Ljava/lang/Class;)Lcom/kamcord/a/a/a/KC_a;
    .locals 1
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/lang/Class",
            "<+",
            "Lcom/kamcord/a/a/a/a/KC_a;",
            ">;)",
            "Lcom/kamcord/a/a/a/KC_a;"
        }
    .end annotation

    invoke-static {p1}, Lcom/kamcord/a/a/a/KC_a;->b(Ljava/lang/Class;)Lcom/kamcord/a/a/a/a/KC_a;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/a/a/a/KC_a;->d:Lcom/kamcord/a/a/a/a/KC_a;

    return-object p0
.end method

.method public final a(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;
    .locals 1

    const-string v0, "Callback can\'t be null"

    invoke-static {p1, v0}, Lcom/kamcord/a/a/g/KC_b;->a(Ljava/lang/Object;Ljava/lang/String;)V

    iput-object p1, p0, Lcom/kamcord/a/a/a/KC_a;->c:Ljava/lang/String;

    return-object p0
.end method

.method public final a()Lcom/kamcord/a/a/e/KC_c;
    .locals 8

    iget-object v0, p0, Lcom/kamcord/a/a/a/KC_a;->d:Lcom/kamcord/a/a/a/a/KC_a;

    const-string v1, "You must specify a valid api through the provider() method"

    invoke-static {v0, v1}, Lcom/kamcord/a/a/g/KC_b;->a(Ljava/lang/Object;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/a/a/a/KC_a;->a:Ljava/lang/String;

    const-string v1, "You must provide an api key"

    invoke-static {v0, v1}, Lcom/kamcord/a/a/g/KC_b;->a(Ljava/lang/String;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/a/a/a/KC_a;->b:Ljava/lang/String;

    const-string v1, "You must provide an api secret"

    invoke-static {v0, v1}, Lcom/kamcord/a/a/g/KC_b;->a(Ljava/lang/String;Ljava/lang/String;)V

    iget-object v7, p0, Lcom/kamcord/a/a/a/KC_a;->d:Lcom/kamcord/a/a/a/a/KC_a;

    new-instance v0, Lcom/kamcord/a/a/d/KC_a;

    iget-object v1, p0, Lcom/kamcord/a/a/a/KC_a;->a:Ljava/lang/String;

    iget-object v2, p0, Lcom/kamcord/a/a/a/KC_a;->b:Ljava/lang/String;

    iget-object v3, p0, Lcom/kamcord/a/a/a/KC_a;->c:Ljava/lang/String;

    iget-object v4, p0, Lcom/kamcord/a/a/a/KC_a;->f:Lcom/kamcord/a/a/d/KC_i;

    iget-object v5, p0, Lcom/kamcord/a/a/a/KC_a;->e:Ljava/lang/String;

    const/4 v6, 0x0

    invoke-direct/range {v0 .. v6}, Lcom/kamcord/a/a/d/KC_a;-><init>(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Lcom/kamcord/a/a/d/KC_i;Ljava/lang/String;Ljava/io/OutputStream;)V

    invoke-interface {v7, v0}, Lcom/kamcord/a/a/a/a/KC_a;->a(Lcom/kamcord/a/a/d/KC_a;)Lcom/kamcord/a/a/e/KC_c;

    move-result-object v0

    return-object v0
.end method

.method public final b(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;
    .locals 1

    const-string v0, "Invalid Api key"

    invoke-static {p1, v0}, Lcom/kamcord/a/a/g/KC_b;->a(Ljava/lang/String;Ljava/lang/String;)V

    iput-object p1, p0, Lcom/kamcord/a/a/a/KC_a;->a:Ljava/lang/String;

    return-object p0
.end method

.method public final c(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;
    .locals 1

    const-string v0, "Invalid Api secret"

    invoke-static {p1, v0}, Lcom/kamcord/a/a/g/KC_b;->a(Ljava/lang/String;Ljava/lang/String;)V

    iput-object p1, p0, Lcom/kamcord/a/a/a/KC_a;->b:Ljava/lang/String;

    return-object p0
.end method

.method public final d(Ljava/lang/String;)Lcom/kamcord/a/a/a/KC_a;
    .locals 1

    const-string v0, "Invalid OAuth scope"

    invoke-static {p1, v0}, Lcom/kamcord/a/a/g/KC_b;->a(Ljava/lang/String;Ljava/lang/String;)V

    iput-object p1, p0, Lcom/kamcord/a/a/a/KC_a;->e:Ljava/lang/String;

    return-object p0
.end method
