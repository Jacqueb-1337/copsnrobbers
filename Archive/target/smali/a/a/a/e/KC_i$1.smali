.class final La/a/a/e/KC_i$1;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements Ljava/util/Comparator;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/e/KC_i;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = null
.end annotation

.annotation system Ldalvik/annotation/Signature;
    value = {
        "Ljava/lang/Object;",
        "Ljava/util/Comparator",
        "<",
        "La/a/a/e/KC_i$KC_b;",
        ">;"
    }
.end annotation


# direct methods
.method constructor <init>()V
    .locals 0

    .prologue
    .line 121
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final bridge synthetic compare(Ljava/lang/Object;Ljava/lang/Object;)I
    .locals 2
    .param p1, "x0"    # Ljava/lang/Object;
    .param p2, "x1"    # Ljava/lang/Object;

    .prologue
    .line 121
    check-cast p1, La/a/a/e/KC_i$KC_b;

    .end local p1    # "x0":Ljava/lang/Object;
    check-cast p2, La/a/a/e/KC_i$KC_b;

    .end local p2    # "x1":Ljava/lang/Object;
    iget v0, p1, La/a/a/e/KC_i$KC_b;->b:I

    iget v1, p2, La/a/a/e/KC_i$KC_b;->b:I

    sub-int/2addr v0, v1

    return v0
.end method
