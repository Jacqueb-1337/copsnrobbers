.class public final La/a/a/e/KC_g;
.super Ljava/lang/Object;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/e/KC_g$KC_g;,
        La/a/a/e/KC_g$KC_f;,
        La/a/a/e/KC_g$KC_e;,
        La/a/a/e/KC_g$KC_d;,
        La/a/a/e/KC_g$KC_c;,
        La/a/a/e/KC_g$KC_b;,
        La/a/a/e/KC_g$KC_a;,
        La/a/a/e/KC_g$KC_h;
    }
.end annotation


# static fields
.field private static a:La/a/a/e/KC_g$KC_h;


# direct methods
.method static constructor <clinit>()V
    .locals 2

    .prologue
    .line 981
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    .line 982
    const/16 v1, 0x13

    if-lt v0, v1, :cond_0

    .line 983
    new-instance v0, La/a/a/e/KC_g$KC_g;

    invoke-direct {v0}, La/a/a/e/KC_g$KC_g;-><init>()V

    sput-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    .line 997
    :goto_0
    return-void

    .line 984
    :cond_0
    const/16 v1, 0x11

    if-lt v0, v1, :cond_1

    .line 985
    new-instance v0, La/a/a/e/KC_g$KC_f;

    invoke-direct {v0}, La/a/a/e/KC_g$KC_f;-><init>()V

    sput-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    goto :goto_0

    .line 986
    :cond_1
    const/16 v1, 0x10

    if-lt v0, v1, :cond_2

    .line 987
    new-instance v0, La/a/a/e/KC_g$KC_e;

    invoke-direct {v0}, La/a/a/e/KC_g$KC_e;-><init>()V

    sput-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    goto :goto_0

    .line 988
    :cond_2
    const/16 v1, 0xe

    if-lt v0, v1, :cond_3

    .line 989
    new-instance v0, La/a/a/e/KC_g$KC_d;

    invoke-direct {v0}, La/a/a/e/KC_g$KC_d;-><init>()V

    sput-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    goto :goto_0

    .line 990
    :cond_3
    const/16 v1, 0xb

    if-lt v0, v1, :cond_4

    .line 991
    new-instance v0, La/a/a/e/KC_g$KC_c;

    invoke-direct {v0}, La/a/a/e/KC_g$KC_c;-><init>()V

    sput-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    goto :goto_0

    .line 992
    :cond_4
    const/16 v1, 0x9

    if-lt v0, v1, :cond_5

    .line 993
    new-instance v0, La/a/a/e/KC_g$KC_b;

    invoke-direct {v0}, La/a/a/e/KC_g$KC_b;-><init>()V

    sput-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    goto :goto_0

    .line 995
    :cond_5
    new-instance v0, La/a/a/e/KC_g$KC_h;

    invoke-direct {v0}, La/a/a/e/KC_g$KC_h;-><init>()V

    sput-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    goto :goto_0
.end method

.method public static a(Landroid/view/View;)I
    .locals 1

    .prologue
    .line 1032
    sget-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    invoke-virtual {v0, p0}, La/a/a/e/KC_g$KC_h;->a(Landroid/view/View;)I

    move-result v0

    return v0
.end method

.method public static a(Landroid/view/View;ILandroid/graphics/Paint;)V
    .locals 2

    .prologue
    .line 1395
    sget-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    const/4 v1, 0x0

    invoke-virtual {v0, p0, p1, v1}, La/a/a/e/KC_g$KC_h;->a(Landroid/view/View;ILandroid/graphics/Paint;)V

    .line 1396
    return-void
.end method

.method public static a(Landroid/view/View;La/a/a/e/KC_a;)V
    .locals 1

    .prologue
    .line 1169
    sget-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    invoke-virtual {v0, p0, p1}, La/a/a/e/KC_g$KC_h;->a(Landroid/view/View;La/a/a/e/KC_a;)V

    .line 1170
    return-void
.end method

.method public static a(Landroid/view/View;Ljava/lang/Runnable;)V
    .locals 1

    .prologue
    .line 1237
    sget-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    invoke-virtual {v0, p0, p1}, La/a/a/e/KC_g$KC_h;->a(Landroid/view/View;Ljava/lang/Runnable;)V

    .line 1238
    return-void
.end method

.method public static a(Landroid/view/View;I)Z
    .locals 1

    .prologue
    .line 1007
    sget-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    invoke-virtual {v0, p0, p1}, La/a/a/e/KC_g$KC_h;->a(Landroid/view/View;I)Z

    move-result v0

    return v0
.end method

.method public static b(Landroid/view/View;)V
    .locals 1

    .prologue
    .line 1205
    sget-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    invoke-virtual {v0, p0}, La/a/a/e/KC_g$KC_h;->b(Landroid/view/View;)V

    .line 1206
    return-void
.end method

.method public static b(Landroid/view/View;I)V
    .locals 2

    .prologue
    .line 1296
    sget-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    const/4 v1, 0x1

    invoke-virtual {v0, p0, v1}, La/a/a/e/KC_g$KC_h;->b(Landroid/view/View;I)V

    .line 1297
    return-void
.end method

.method public static c(Landroid/view/View;)I
    .locals 1

    .prologue
    .line 1272
    sget-object v0, La/a/a/e/KC_g;->a:La/a/a/e/KC_g$KC_h;

    invoke-virtual {v0, p0}, La/a/a/e/KC_g$KC_h;->c(Landroid/view/View;)I

    move-result v0

    return v0
.end method
