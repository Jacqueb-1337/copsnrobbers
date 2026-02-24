.class final La/a/a/e/KC_h$KC_a;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements La/a/a/e/KC_h$KC_c;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/e/KC_h;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_a"
.end annotation


# direct methods
.method constructor <init>()V
    .locals 0

    .prologue
    .line 36
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(Landroid/view/ViewConfiguration;)I
    .locals 1

    .prologue
    .line 39
    invoke-virtual {p1}, Landroid/view/ViewConfiguration;->getScaledTouchSlop()I

    move-result v0

    return v0
.end method
