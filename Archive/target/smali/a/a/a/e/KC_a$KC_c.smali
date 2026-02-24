.class final La/a/a/e/KC_a$KC_c;
.super La/a/a/e/KC_a$KC_a;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/e/KC_a;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_c"
.end annotation


# direct methods
.method constructor <init>()V
    .locals 0

    .prologue
    .line 214
    invoke-direct {p0}, La/a/a/e/KC_a$KC_a;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(Ljava/lang/Object;Landroid/view/View;)La/a/a/e/a/KC_c;
    .locals 2

    .prologue
    .line 275
    check-cast p1, Landroid/view/View$AccessibilityDelegate;

    invoke-virtual {p1, p2}, Landroid/view/View$AccessibilityDelegate;->getAccessibilityNodeProvider(Landroid/view/View;)Landroid/view/accessibility/AccessibilityNodeProvider;

    move-result-object v1

    .line 277
    if-eqz v1, :cond_0

    .line 278
    new-instance v0, La/a/a/e/a/KC_c;

    invoke-direct {v0, v1}, La/a/a/e/a/KC_c;-><init>(Ljava/lang/Object;)V

    .line 280
    :goto_0
    return-object v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public final a(La/a/a/e/KC_a;)Ljava/lang/Object;
    .locals 2

    .prologue
    .line 217
    new-instance v0, La/a/a/e/KC_a$KC_c$KC_a;

    invoke-direct {v0, p0, p1}, La/a/a/e/KC_a$KC_c$KC_a;-><init>(La/a/a/e/KC_a$KC_c;La/a/a/e/KC_a;)V

    new-instance v1, La/a/a/e/KC_b;

    invoke-direct {v1, v0}, La/a/a/e/KC_b;-><init>(La/a/a/e/KC_a$KC_c$KC_a;)V

    return-object v1
.end method

.method public final a(Ljava/lang/Object;Landroid/view/View;ILandroid/os/Bundle;)Z
    .locals 1

    .prologue
    .line 286
    check-cast p1, Landroid/view/View$AccessibilityDelegate;

    invoke-virtual {p1, p2, p3, p4}, Landroid/view/View$AccessibilityDelegate;->performAccessibilityAction(Landroid/view/View;ILandroid/os/Bundle;)Z

    move-result v0

    return v0
.end method
