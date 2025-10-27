.class public final La/a/a/e/KC_c;
.super Ljava/lang/Object;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/e/KC_c$KC_b;,
        La/a/a/e/KC_c$KC_a;,
        La/a/a/e/KC_c$KC_c;
    }
.end annotation


# static fields
.field private static a:La/a/a/e/KC_c$KC_c;


# direct methods
.method static constructor <clinit>()V
    .locals 2

    .prologue
    .line 166
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0xb

    if-lt v0, v1, :cond_0

    .line 167
    new-instance v0, La/a/a/e/KC_c$KC_b;

    invoke-direct {v0}, La/a/a/e/KC_c$KC_b;-><init>()V

    sput-object v0, La/a/a/e/KC_c;->a:La/a/a/e/KC_c$KC_c;

    .line 171
    :goto_0
    return-void

    .line 169
    :cond_0
    new-instance v0, La/a/a/e/KC_c$KC_c;

    invoke-direct {v0}, La/a/a/e/KC_c$KC_c;-><init>()V

    sput-object v0, La/a/a/e/KC_c;->a:La/a/a/e/KC_c$KC_c;

    goto :goto_0
.end method

.method public static a(Landroid/view/KeyEvent;)Z
    .locals 2

    .prologue
    .line 192
    sget-object v0, La/a/a/e/KC_c;->a:La/a/a/e/KC_c$KC_c;

    invoke-virtual {p0}, Landroid/view/KeyEvent;->getMetaState()I

    move-result v1

    invoke-virtual {v0, v1}, La/a/a/e/KC_c$KC_c;->b(I)Z

    move-result v0

    return v0
.end method

.method public static a(Landroid/view/KeyEvent;I)Z
    .locals 3

    .prologue
    .line 188
    sget-object v0, La/a/a/e/KC_c;->a:La/a/a/e/KC_c$KC_c;

    invoke-virtual {p0}, Landroid/view/KeyEvent;->getMetaState()I

    move-result v1

    const/4 v2, 0x1

    invoke-virtual {v0, v1, v2}, La/a/a/e/KC_c$KC_c;->a(II)Z

    move-result v0

    return v0
.end method
