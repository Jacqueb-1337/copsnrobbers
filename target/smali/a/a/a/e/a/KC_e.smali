.class final La/a/a/e/a/KC_e;
.super Landroid/view/accessibility/AccessibilityNodeProvider;
.source "SourceFile"


# instance fields
.field private synthetic a:La/a/a/e/a/KC_c$KC_b$KC_a;


# direct methods
.method constructor <init>(La/a/a/e/a/KC_c$KC_b$KC_a;)V
    .locals 0

    .prologue
    .line 39
    iput-object p1, p0, La/a/a/e/a/KC_e;->a:La/a/a/e/a/KC_c$KC_b$KC_a;

    invoke-direct {p0}, Landroid/view/accessibility/AccessibilityNodeProvider;-><init>()V

    return-void
.end method


# virtual methods
.method public final createAccessibilityNodeInfo(I)Landroid/view/accessibility/AccessibilityNodeInfo;
    .locals 1
    .param p1, "virtualViewId"    # I

    .prologue
    .line 42
    iget-object v0, p0, La/a/a/e/a/KC_e;->a:La/a/a/e/a/KC_c$KC_b$KC_a;

    invoke-virtual {v0, p1}, La/a/a/e/a/KC_c$KC_b$KC_a;->a(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/view/accessibility/AccessibilityNodeInfo;

    return-object v0
.end method

.method public final findAccessibilityNodeInfosByText(Ljava/lang/String;I)Ljava/util/List;
    .locals 1
    .param p1, "text"    # Ljava/lang/String;
    .param p2, "virtualViewId"    # I
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/lang/String;",
            "I)",
            "Ljava/util/List",
            "<",
            "Landroid/view/accessibility/AccessibilityNodeInfo;",
            ">;"
        }
    .end annotation

    .prologue
    .line 50
    iget-object v0, p0, La/a/a/e/a/KC_e;->a:La/a/a/e/a/KC_c$KC_b$KC_a;

    invoke-virtual {v0, p1, p2}, La/a/a/e/a/KC_c$KC_b$KC_a;->a(Ljava/lang/String;I)Ljava/util/List;

    move-result-object v0

    return-object v0
.end method

.method public final findFocus(I)Landroid/view/accessibility/AccessibilityNodeInfo;
    .locals 1
    .param p1, "focus"    # I

    .prologue
    .line 61
    iget-object v0, p0, La/a/a/e/a/KC_e;->a:La/a/a/e/a/KC_c$KC_b$KC_a;

    invoke-virtual {v0, p1}, La/a/a/e/a/KC_c$KC_b$KC_a;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/view/accessibility/AccessibilityNodeInfo;

    return-object v0
.end method

.method public final performAction(IILandroid/os/Bundle;)Z
    .locals 1
    .param p1, "virtualViewId"    # I
    .param p2, "action"    # I
    .param p3, "arguments"    # Landroid/os/Bundle;

    .prologue
    .line 56
    iget-object v0, p0, La/a/a/e/a/KC_e;->a:La/a/a/e/a/KC_c$KC_b$KC_a;

    invoke-virtual {v0, p1, p2, p3}, La/a/a/e/a/KC_c$KC_b$KC_a;->a(IILandroid/os/Bundle;)Z

    move-result v0

    return v0
.end method
