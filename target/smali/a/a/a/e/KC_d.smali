.class public final La/a/a/e/KC_d;
.super Ljava/lang/Object;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/e/KC_d$KC_b;,
        La/a/a/e/KC_d$KC_a;,
        La/a/a/e/KC_d$KC_c;
    }
.end annotation


# static fields
.field private static a:La/a/a/e/KC_d$KC_c;


# direct methods
.method static constructor <clinit>()V
    .locals 2

    .prologue
    .line 108
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/4 v1, 0x5

    if-lt v0, v1, :cond_0

    .line 109
    new-instance v0, La/a/a/e/KC_d$KC_b;

    invoke-direct {v0}, La/a/a/e/KC_d$KC_b;-><init>()V

    sput-object v0, La/a/a/e/KC_d;->a:La/a/a/e/KC_d$KC_c;

    .line 113
    :goto_0
    return-void

    .line 111
    :cond_0
    new-instance v0, La/a/a/e/KC_d$KC_a;

    invoke-direct {v0}, La/a/a/e/KC_d$KC_a;-><init>()V

    sput-object v0, La/a/a/e/KC_d;->a:La/a/a/e/KC_d$KC_c;

    goto :goto_0
.end method

.method public static a(Landroid/view/MotionEvent;)I
    .locals 1

    .prologue
    .line 191
    invoke-virtual {p0}, Landroid/view/MotionEvent;->getAction()I

    move-result v0

    shr-int/lit8 v0, v0, 0x8

    and-int/lit16 v0, v0, 0xff

    return v0
.end method

.method public static a(Landroid/view/MotionEvent;I)I
    .locals 1

    .prologue
    .line 201
    sget-object v0, La/a/a/e/KC_d;->a:La/a/a/e/KC_d$KC_c;

    invoke-interface {v0, p0, p1}, La/a/a/e/KC_d$KC_c;->a(Landroid/view/MotionEvent;I)I

    move-result v0

    return v0
.end method

.method public static b(Landroid/view/MotionEvent;I)I
    .locals 1

    .prologue
    .line 210
    sget-object v0, La/a/a/e/KC_d;->a:La/a/a/e/KC_d$KC_c;

    invoke-interface {v0, p0, p1}, La/a/a/e/KC_d$KC_c;->b(Landroid/view/MotionEvent;I)I

    move-result v0

    return v0
.end method

.method public static c(Landroid/view/MotionEvent;I)F
    .locals 1

    .prologue
    .line 219
    sget-object v0, La/a/a/e/KC_d;->a:La/a/a/e/KC_d$KC_c;

    invoke-interface {v0, p0, p1}, La/a/a/e/KC_d$KC_c;->c(Landroid/view/MotionEvent;I)F

    move-result v0

    return v0
.end method

.method public static d(Landroid/view/MotionEvent;I)F
    .locals 1

    .prologue
    .line 228
    sget-object v0, La/a/a/e/KC_d;->a:La/a/a/e/KC_d$KC_c;

    invoke-interface {v0, p0, p1}, La/a/a/e/KC_d$KC_c;->d(Landroid/view/MotionEvent;I)F

    move-result v0

    return v0
.end method
