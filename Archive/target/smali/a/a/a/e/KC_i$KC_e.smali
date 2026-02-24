.class final La/a/a/e/KC_i$KC_e;
.super Landroid/database/DataSetObserver;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/e/KC_i;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = "KC_e"
.end annotation


# instance fields
.field private synthetic a:La/a/a/e/KC_i;


# direct methods
.method private constructor <init>(La/a/a/e/KC_i;)V
    .locals 0

    .prologue
    .line 2821
    iput-object p1, p0, La/a/a/e/KC_i$KC_e;->a:La/a/a/e/KC_i;

    invoke-direct {p0}, Landroid/database/DataSetObserver;-><init>()V

    return-void
.end method

.method synthetic constructor <init>(La/a/a/e/KC_i;B)V
    .locals 0

    .prologue
    .line 2821
    invoke-direct {p0, p1}, La/a/a/e/KC_i$KC_e;-><init>(La/a/a/e/KC_i;)V

    return-void
.end method


# virtual methods
.method public final onChanged()V
    .locals 1

    .prologue
    .line 2824
    iget-object v0, p0, La/a/a/e/KC_i$KC_e;->a:La/a/a/e/KC_i;

    invoke-virtual {v0}, La/a/a/e/KC_i;->a()V

    .line 2825
    return-void
.end method

.method public final onInvalidated()V
    .locals 1

    .prologue
    .line 2828
    iget-object v0, p0, La/a/a/e/KC_i$KC_e;->a:La/a/a/e/KC_i;

    invoke-virtual {v0}, La/a/a/e/KC_i;->a()V

    .line 2829
    return-void
.end method
