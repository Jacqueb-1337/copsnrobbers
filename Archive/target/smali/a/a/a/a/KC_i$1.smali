.class final La/a/a/a/KC_i$1;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/a/KC_i;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:La/a/a/a/KC_i;


# direct methods
.method constructor <init>(La/a/a/a/KC_i;)V
    .locals 0

    .prologue
    .line 447
    iput-object p1, p0, La/a/a/a/KC_i$1;->a:La/a/a/a/KC_i;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 1

    .prologue
    .line 450
    iget-object v0, p0, La/a/a/a/KC_i$1;->a:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->d()Z

    .line 451
    return-void
.end method
