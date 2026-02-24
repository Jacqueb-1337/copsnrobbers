.class final La/a/a/d/KC_a$1;
.super La/a/a/d/KC_d;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = La/a/a/d/KC_a;->b()La/a/a/d/KC_d;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation

.annotation system Ldalvik/annotation/Signature;
    value = {
        "La/a/a/d/KC_d",
        "<TK;TV;>;"
    }
.end annotation


# instance fields
.field private synthetic d:La/a/a/d/KC_a;


# direct methods
.method constructor <init>(La/a/a/d/KC_a;)V
    .locals 0

    .prologue
    .line 73
    iput-object p1, p0, La/a/a/d/KC_a$1;->d:La/a/a/d/KC_a;

    invoke-direct {p0}, La/a/a/d/KC_d;-><init>()V

    return-void
.end method


# virtual methods
.method protected final a()I
    .locals 1

    .prologue
    .line 76
    iget-object v0, p0, La/a/a/d/KC_a$1;->d:La/a/a/d/KC_a;

    iget v0, v0, La/a/a/d/KC_a;->b:I

    return v0
.end method

.method protected final a(Ljava/lang/Object;)I
    .locals 2

    .prologue
    .line 86
    if-nez p1, :cond_0

    iget-object v0, p0, La/a/a/d/KC_a$1;->d:La/a/a/d/KC_a;

    invoke-virtual {v0}, La/a/a/d/KC_a;->a()I

    move-result v0

    :goto_0
    return v0

    :cond_0
    iget-object v0, p0, La/a/a/d/KC_a$1;->d:La/a/a/d/KC_a;

    invoke-virtual {p1}, Ljava/lang/Object;->hashCode()I

    move-result v1

    invoke-virtual {v0, p1, v1}, La/a/a/d/KC_a;->a(Ljava/lang/Object;I)I

    move-result v0

    goto :goto_0
.end method

.method protected final a(II)Ljava/lang/Object;
    .locals 2

    .prologue
    .line 81
    iget-object v0, p0, La/a/a/d/KC_a$1;->d:La/a/a/d/KC_a;

    iget-object v0, v0, La/a/a/d/KC_a;->a:[Ljava/lang/Object;

    shl-int/lit8 v1, p1, 0x1

    add-int/2addr v1, p2

    aget-object v0, v0, v1

    return-object v0
.end method

.method protected final a(ILjava/lang/Object;)Ljava/lang/Object;
    .locals 3
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(ITV;)TV;"
        }
    .end annotation

    .prologue
    .line 106
    iget-object v0, p0, La/a/a/d/KC_a$1;->d:La/a/a/d/KC_a;

    shl-int/lit8 v1, p1, 0x1

    add-int/lit8 v1, v1, 0x1

    iget-object v2, v0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    aget-object v2, v2, v1

    iget-object v0, v0, La/a/a/d/KC_e;->a:[Ljava/lang/Object;

    aput-object p2, v0, v1

    return-object v2
.end method

.method protected final a(I)V
    .locals 1

    .prologue
    .line 111
    iget-object v0, p0, La/a/a/d/KC_a$1;->d:La/a/a/d/KC_a;

    invoke-virtual {v0, p1}, La/a/a/d/KC_a;->c(I)Ljava/lang/Object;

    .line 112
    return-void
.end method

.method protected final a(Ljava/lang/Object;Ljava/lang/Object;)V
    .locals 1
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(TK;TV;)V"
        }
    .end annotation

    .prologue
    .line 101
    iget-object v0, p0, La/a/a/d/KC_a$1;->d:La/a/a/d/KC_a;

    invoke-virtual {v0, p1, p2}, La/a/a/d/KC_a;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    .line 102
    return-void
.end method

.method protected final b(Ljava/lang/Object;)I
    .locals 1

    .prologue
    .line 91
    iget-object v0, p0, La/a/a/d/KC_a$1;->d:La/a/a/d/KC_a;

    invoke-virtual {v0, p1}, La/a/a/d/KC_a;->a(Ljava/lang/Object;)I

    move-result v0

    return v0
.end method

.method protected final b()Ljava/util/Map;
    .locals 1
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "()",
            "Ljava/util/Map",
            "<TK;TV;>;"
        }
    .end annotation

    .prologue
    .line 96
    iget-object v0, p0, La/a/a/d/KC_a$1;->d:La/a/a/d/KC_a;

    return-object v0
.end method

.method protected final c()V
    .locals 1

    .prologue
    .line 116
    iget-object v0, p0, La/a/a/d/KC_a$1;->d:La/a/a/d/KC_a;

    invoke-virtual {v0}, La/a/a/d/KC_a;->clear()V

    .line 117
    return-void
.end method
