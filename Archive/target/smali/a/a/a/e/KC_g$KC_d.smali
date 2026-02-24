.class La/a/a/e/KC_g$KC_d;
.super La/a/a/e/KC_g$KC_c;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/e/KC_g;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_d"
.end annotation


# direct methods
.method constructor <init>()V
    .locals 0

    .prologue
    .line 811
    invoke-direct {p0}, La/a/a/e/KC_g$KC_c;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(Landroid/view/View;La/a/a/e/KC_a;)V
    .locals 1

    .prologue
    .line 834
    invoke-virtual {p2}, La/a/a/e/KC_a;->a()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/view/View$AccessibilityDelegate;

    invoke-virtual {p1, v0}, Landroid/view/View;->setAccessibilityDelegate(Landroid/view/View$AccessibilityDelegate;)V

    .line 835
    return-void
.end method

.method public final a(Landroid/view/View;I)Z
    .locals 1

    .prologue
    .line 814
    invoke-virtual {p1, p2}, Landroid/view/View;->canScrollHorizontally(I)Z

    move-result v0

    return v0
.end method
