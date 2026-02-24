.class public La/a/a/d/KC_a;
.super La/a/a/d/KC_e;
.source "SourceFile"

# interfaces
.implements Ljava/util/Map;


# annotations
.annotation system Ldalvik/annotation/Signature;
    value = {
        "<K:",
        "Ljava/lang/Object;",
        "V:",
        "Ljava/lang/Object;",
        ">",
        "La/a/a/d/KC_e",
        "<TK;TV;>;",
        "Ljava/util/Map",
        "<TK;TV;>;"
    }
.end annotation


# instance fields
.field private c:La/a/a/d/KC_d;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "La/a/a/d/KC_d",
            "<TK;TV;>;"
        }
    .end annotation
.end field


# direct methods
.method public constructor <init>()V
    .locals 0

    .prologue
    .line 54
    invoke-direct {p0}, La/a/a/d/KC_e;-><init>()V

    .line 55
    return-void
.end method

.method private b()La/a/a/d/KC_d;
    .locals 1
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "()",
            "La/a/a/d/KC_d",
            "<TK;TV;>;"
        }
    .end annotation

    .prologue
    .line 72
    iget-object v0, p0, La/a/a/d/KC_a;->c:La/a/a/d/KC_d;

    if-nez v0, :cond_0

    .line 73
    new-instance v0, La/a/a/d/KC_a$1;

    invoke-direct {v0, p0}, La/a/a/d/KC_a$1;-><init>(La/a/a/d/KC_a;)V

    iput-object v0, p0, La/a/a/d/KC_a;->c:La/a/a/d/KC_d;

    .line 120
    :cond_0
    iget-object v0, p0, La/a/a/d/KC_a;->c:La/a/a/d/KC_d;

    return-object v0
.end method


# virtual methods
.method public entrySet()Ljava/util/Set;
    .locals 2
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "()",
            "Ljava/util/Set",
            "<",
            "Ljava/util/Map$Entry",
            "<TK;TV;>;>;"
        }
    .end annotation

    .prologue
    .line 179
    invoke-direct {p0}, La/a/a/d/KC_a;->b()La/a/a/d/KC_d;

    move-result-object v0

    iget-object v1, v0, La/a/a/d/KC_d;->a:La/a/a/d/KC_d$KC_b;

    if-nez v1, :cond_0

    new-instance v1, La/a/a/d/KC_d$KC_b;

    invoke-direct {v1, v0}, La/a/a/d/KC_d$KC_b;-><init>(La/a/a/d/KC_d;)V

    iput-object v1, v0, La/a/a/d/KC_d;->a:La/a/a/d/KC_d$KC_b;

    :cond_0
    iget-object v0, v0, La/a/a/d/KC_d;->a:La/a/a/d/KC_d$KC_b;

    return-object v0
.end method

.method public keySet()Ljava/util/Set;
    .locals 2
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "()",
            "Ljava/util/Set",
            "<TK;>;"
        }
    .end annotation

    .prologue
    .line 191
    invoke-direct {p0}, La/a/a/d/KC_a;->b()La/a/a/d/KC_d;

    move-result-object v0

    iget-object v1, v0, La/a/a/d/KC_d;->b:La/a/a/d/KC_d$KC_c;

    if-nez v1, :cond_0

    new-instance v1, La/a/a/d/KC_d$KC_c;

    invoke-direct {v1, v0}, La/a/a/d/KC_d$KC_c;-><init>(La/a/a/d/KC_d;)V

    iput-object v1, v0, La/a/a/d/KC_d;->b:La/a/a/d/KC_d$KC_c;

    :cond_0
    iget-object v0, v0, La/a/a/d/KC_d;->b:La/a/a/d/KC_d$KC_c;

    return-object v0
.end method

.method public putAll(Ljava/util/Map;)V
    .locals 3
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/util/Map",
            "<+TK;+TV;>;)V"
        }
    .end annotation

    .prologue
    .line 139
    .local p0, "this":La/a/a/d/KC_a;, "La/a/a/d/KC_a<TK;TV;>;"
    .local p1, "map":Ljava/util/Map;, "Ljava/util/Map<+TK;+TV;>;"
    iget v0, p0, La/a/a/d/KC_a;->b:I

    invoke-interface {p1}, Ljava/util/Map;->size()I

    move-result v1

    add-int/2addr v0, v1

    invoke-virtual {p0, v0}, La/a/a/d/KC_a;->a(I)V

    .line 140
    invoke-interface {p1}, Ljava/util/Map;->entrySet()Ljava/util/Set;

    move-result-object v0

    invoke-interface {v0}, Ljava/util/Set;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Map$Entry;

    .line 141
    invoke-interface {v0}, Ljava/util/Map$Entry;->getKey()Ljava/lang/Object;

    move-result-object v2

    invoke-interface {v0}, Ljava/util/Map$Entry;->getValue()Ljava/lang/Object;

    move-result-object v0

    invoke-virtual {p0, v2, v0}, La/a/a/d/KC_a;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    goto :goto_0

    .line 143
    :cond_0
    return-void
.end method

.method public values()Ljava/util/Collection;
    .locals 2
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "()",
            "Ljava/util/Collection",
            "<TV;>;"
        }
    .end annotation

    .prologue
    .line 203
    invoke-direct {p0}, La/a/a/d/KC_a;->b()La/a/a/d/KC_d;

    move-result-object v0

    iget-object v1, v0, La/a/a/d/KC_d;->c:La/a/a/d/KC_d$KC_e;

    if-nez v1, :cond_0

    new-instance v1, La/a/a/d/KC_d$KC_e;

    invoke-direct {v1, v0}, La/a/a/d/KC_d$KC_e;-><init>(La/a/a/d/KC_d;)V

    iput-object v1, v0, La/a/a/d/KC_d;->c:La/a/a/d/KC_d$KC_e;

    :cond_0
    iget-object v0, v0, La/a/a/d/KC_d;->c:La/a/a/d/KC_d$KC_e;

    return-object v0
.end method
