.class final La/a/a/a/KC_i$2;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = La/a/a/a/KC_i;->a(II)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:I

.field private synthetic b:I

.field private synthetic c:La/a/a/a/KC_i;


# direct methods
.method constructor <init>(La/a/a/a/KC_i;II)V
    .locals 0

    .prologue
    .line 522
    iput-object p1, p0, La/a/a/a/KC_i$2;->c:La/a/a/a/KC_i;

    iput p2, p0, La/a/a/a/KC_i$2;->a:I

    iput p3, p0, La/a/a/a/KC_i$2;->b:I

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 4

    .prologue
    .line 524
    iget-object v0, p0, La/a/a/a/KC_i$2;->c:La/a/a/a/KC_i;

    iget-object v1, p0, La/a/a/a/KC_i$2;->c:La/a/a/a/KC_i;

    iget-object v1, v1, La/a/a/a/KC_i;->e:La/a/a/a/KC_f;

    iget-object v1, v1, La/a/a/a/KC_f;->a:Landroid/os/Handler;

    const/4 v1, 0x0

    iget v2, p0, La/a/a/a/KC_i$2;->a:I

    iget v3, p0, La/a/a/a/KC_i$2;->b:I

    invoke-virtual {v0, v1, v2, v3}, La/a/a/a/KC_i;->a(Ljava/lang/String;II)Z

    .line 525
    return-void
.end method
