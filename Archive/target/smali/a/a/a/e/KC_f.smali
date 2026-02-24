.class public final La/a/a/e/KC_f;
.super Ljava/lang/Object;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/e/KC_f$KC_b;,
        La/a/a/e/KC_f$KC_a;,
        La/a/a/e/KC_f$KC_c;
    }
.end annotation


# static fields
.field private static a:La/a/a/e/KC_f$KC_c;


# direct methods
.method static constructor <clinit>()V
    .locals 2

    .prologue
    .line 67
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0xb

    if-lt v0, v1, :cond_0

    .line 68
    new-instance v0, La/a/a/e/KC_f$KC_b;

    invoke-direct {v0}, La/a/a/e/KC_f$KC_b;-><init>()V

    sput-object v0, La/a/a/e/KC_f;->a:La/a/a/e/KC_f$KC_c;

    .line 72
    :goto_0
    return-void

    .line 70
    :cond_0
    new-instance v0, La/a/a/e/KC_f$KC_a;

    invoke-direct {v0}, La/a/a/e/KC_f$KC_a;-><init>()V

    sput-object v0, La/a/a/e/KC_f;->a:La/a/a/e/KC_f$KC_c;

    goto :goto_0
.end method

.method public static a(Landroid/view/VelocityTracker;I)F
    .locals 1

    .prologue
    .line 82
    sget-object v0, La/a/a/e/KC_f;->a:La/a/a/e/KC_f$KC_c;

    invoke-interface {v0, p0, p1}, La/a/a/e/KC_f$KC_c;->a(Landroid/view/VelocityTracker;I)F

    move-result v0

    return v0
.end method
