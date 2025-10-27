.class La/a/a/e/KC_g$KC_e;
.super La/a/a/e/KC_g$KC_d;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/e/KC_g;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_e"
.end annotation


# direct methods
.method constructor <init>()V
    .locals 0

    .prologue
    .line 851
    invoke-direct {p0}, La/a/a/e/KC_g$KC_d;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(Landroid/view/View;Ljava/lang/Runnable;)V
    .locals 0

    .prologue
    .line 870
    invoke-virtual {p1, p2}, Landroid/view/View;->postOnAnimation(Ljava/lang/Runnable;)V

    .line 871
    return-void
.end method

.method public final b(Landroid/view/View;)V
    .locals 0

    .prologue
    .line 862
    invoke-virtual {p1}, Landroid/view/View;->postInvalidateOnAnimation()V

    .line 863
    return-void
.end method

.method public b(Landroid/view/View;I)V
    .locals 1

    .prologue
    .line 885
    const/4 v0, 0x4

    if-ne p2, v0, :cond_0

    .line 886
    const/4 p2, 0x2

    .line 888
    :cond_0
    invoke-virtual {p1, p2}, Landroid/view/View;->setImportantForAccessibility(I)V

    .line 889
    return-void
.end method

.method public final c(Landroid/view/View;)I
    .locals 1

    .prologue
    .line 878
    invoke-virtual {p1}, Landroid/view/View;->getImportantForAccessibility()I

    move-result v0

    return v0
.end method
