.class public La/a/a/e/KC_a;
.super Ljava/lang/Object;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/e/KC_a$KC_c;,
        La/a/a/e/KC_a$KC_a;,
        La/a/a/e/KC_a$KC_b;
    }
.end annotation


# static fields
.field private static final a:La/a/a/e/KC_a$KC_b;

.field private static final b:Ljava/lang/Object;


# instance fields
.field private c:Ljava/lang/Object;


# direct methods
.method static constructor <clinit>()V
    .locals 2

    .prologue
    .line 295
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x10

    if-lt v0, v1, :cond_0

    .line 296
    new-instance v0, La/a/a/e/KC_a$KC_c;

    invoke-direct {v0}, La/a/a/e/KC_a$KC_c;-><init>()V

    sput-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    .line 302
    :goto_0
    sget-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    invoke-virtual {v0}, La/a/a/e/KC_a$KC_b;->a()Ljava/lang/Object;

    move-result-object v0

    sput-object v0, La/a/a/e/KC_a;->b:Ljava/lang/Object;

    .line 303
    return-void

    .line 297
    :cond_0
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0xe

    if-lt v0, v1, :cond_1

    .line 298
    new-instance v0, La/a/a/e/KC_a$KC_a;

    invoke-direct {v0}, La/a/a/e/KC_a$KC_a;-><init>()V

    sput-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    goto :goto_0

    .line 300
    :cond_1
    new-instance v0, La/a/a/e/KC_a$KC_b;

    invoke-direct {v0}, La/a/a/e/KC_a$KC_b;-><init>()V

    sput-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    goto :goto_0
.end method

.method public constructor <init>()V
    .locals 1

    .prologue
    .line 310
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 311
    sget-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    invoke-virtual {v0, p0}, La/a/a/e/KC_a$KC_b;->a(La/a/a/e/KC_a;)Ljava/lang/Object;

    move-result-object v0

    iput-object v0, p0, La/a/a/e/KC_a;->c:Ljava/lang/Object;

    .line 312
    return-void
.end method

.method public static a(Landroid/view/View;)La/a/a/e/a/KC_c;
    .locals 2

    .prologue
    .line 480
    sget-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    sget-object v1, La/a/a/e/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p0}, La/a/a/e/KC_a$KC_b;->a(Ljava/lang/Object;Landroid/view/View;)La/a/a/e/a/KC_c;

    move-result-object v0

    return-object v0
.end method

.method public static a(Landroid/view/View;I)V
    .locals 2

    .prologue
    .line 336
    sget-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    sget-object v1, La/a/a/e/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p0, p1}, La/a/a/e/KC_a$KC_b;->a(Ljava/lang/Object;Landroid/view/View;I)V

    .line 337
    return-void
.end method

.method public static a(Landroid/view/View;Landroid/view/accessibility/AccessibilityEvent;)V
    .locals 2

    .prologue
    .line 358
    sget-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    sget-object v1, La/a/a/e/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p0, p1}, La/a/a/e/KC_a$KC_b;->d(Ljava/lang/Object;Landroid/view/View;Landroid/view/accessibility/AccessibilityEvent;)V

    .line 359
    return-void
.end method

.method public static a(Landroid/view/ViewGroup;Landroid/view/View;Landroid/view/accessibility/AccessibilityEvent;)Z
    .locals 2

    .prologue
    .line 462
    sget-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    sget-object v1, La/a/a/e/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p0, p1, p2}, La/a/a/e/KC_a$KC_b;->a(Ljava/lang/Object;Landroid/view/ViewGroup;Landroid/view/View;Landroid/view/accessibility/AccessibilityEvent;)Z

    move-result v0

    return v0
.end method

.method public static b(Landroid/view/View;Landroid/view/accessibility/AccessibilityEvent;)Z
    .locals 2

    .prologue
    .line 379
    sget-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    sget-object v1, La/a/a/e/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p0, p1}, La/a/a/e/KC_a$KC_b;->a(Ljava/lang/Object;Landroid/view/View;Landroid/view/accessibility/AccessibilityEvent;)Z

    move-result v0

    return v0
.end method

.method public static c(Landroid/view/View;Landroid/view/accessibility/AccessibilityEvent;)V
    .locals 2

    .prologue
    .line 399
    sget-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    sget-object v1, La/a/a/e/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p0, p1}, La/a/a/e/KC_a$KC_b;->c(Ljava/lang/Object;Landroid/view/View;Landroid/view/accessibility/AccessibilityEvent;)V

    .line 400
    return-void
.end method


# virtual methods
.method final a()Ljava/lang/Object;
    .locals 1

    .prologue
    .line 318
    iget-object v0, p0, La/a/a/e/KC_a;->c:Ljava/lang/Object;

    return-object v0
.end method

.method public a(Landroid/view/View;La/a/a/e/a/KC_a;)V
    .locals 2

    .prologue
    .line 438
    sget-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    sget-object v1, La/a/a/e/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p1, p2}, La/a/a/e/KC_a$KC_b;->a(Ljava/lang/Object;Landroid/view/View;La/a/a/e/a/KC_a;)V

    .line 439
    return-void
.end method

.method public a(Landroid/view/View;ILandroid/os/Bundle;)Z
    .locals 2

    .prologue
    .line 500
    sget-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    sget-object v1, La/a/a/e/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p1, p2, p3}, La/a/a/e/KC_a$KC_b;->a(Ljava/lang/Object;Landroid/view/View;ILandroid/os/Bundle;)Z

    move-result v0

    return v0
.end method

.method public d(Landroid/view/View;Landroid/view/accessibility/AccessibilityEvent;)V
    .locals 2

    .prologue
    .line 419
    sget-object v0, La/a/a/e/KC_a;->a:La/a/a/e/KC_a$KC_b;

    sget-object v1, La/a/a/e/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p1, p2}, La/a/a/e/KC_a$KC_b;->b(Ljava/lang/Object;Landroid/view/View;Landroid/view/accessibility/AccessibilityEvent;)V

    .line 420
    return-void
.end method
