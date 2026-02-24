.class final Lcom/kamcord/a/a/e/KC_a$KC_a;
.super Lcom/kamcord/a/a/d/KC_g;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/a/a/e/KC_a;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_a"
.end annotation


# instance fields
.field private final a:I

.field private final b:Ljava/util/concurrent/TimeUnit;


# direct methods
.method public constructor <init>(ILjava/util/concurrent/TimeUnit;)V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/a/a/d/KC_g;-><init>()V

    iput p1, p0, Lcom/kamcord/a/a/e/KC_a$KC_a;->a:I

    iput-object p2, p0, Lcom/kamcord/a/a/e/KC_a$KC_a;->b:Ljava/util/concurrent/TimeUnit;

    return-void
.end method


# virtual methods
.method public final a(Lcom/kamcord/a/a/d/KC_f;)V
    .locals 2

    iget v0, p0, Lcom/kamcord/a/a/e/KC_a$KC_a;->a:I

    iget-object v1, p0, Lcom/kamcord/a/a/e/KC_a$KC_a;->b:Ljava/util/concurrent/TimeUnit;

    invoke-virtual {p1, v0, v1}, Lcom/kamcord/a/a/d/KC_f;->a(ILjava/util/concurrent/TimeUnit;)V

    return-void
.end method
