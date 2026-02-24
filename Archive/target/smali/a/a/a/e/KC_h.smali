.class public final La/a/a/e/KC_h;
.super Ljava/lang/Object;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/e/KC_h$KC_b;,
        La/a/a/e/KC_h$KC_a;,
        La/a/a/e/KC_h$KC_c;
    }
.end annotation


# static fields
.field private static a:La/a/a/e/KC_h$KC_c;


# direct methods
.method static constructor <clinit>()V
    .locals 2

    .prologue
    .line 58
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0xb

    if-lt v0, v1, :cond_0

    .line 59
    new-instance v0, La/a/a/e/KC_h$KC_b;

    invoke-direct {v0}, La/a/a/e/KC_h$KC_b;-><init>()V

    sput-object v0, La/a/a/e/KC_h;->a:La/a/a/e/KC_h$KC_c;

    .line 63
    :goto_0
    return-void

    .line 61
    :cond_0
    new-instance v0, La/a/a/e/KC_h$KC_a;

    invoke-direct {v0}, La/a/a/e/KC_h$KC_a;-><init>()V

    sput-object v0, La/a/a/e/KC_h;->a:La/a/a/e/KC_h$KC_c;

    goto :goto_0
.end method

.method public static a(Landroid/view/ViewConfiguration;)I
    .locals 1

    .prologue
    .line 73
    sget-object v0, La/a/a/e/KC_h;->a:La/a/a/e/KC_h$KC_c;

    invoke-interface {v0, p0}, La/a/a/e/KC_h$KC_c;->a(Landroid/view/ViewConfiguration;)I

    move-result v0

    return v0
.end method
