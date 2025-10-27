.class final Lcom/kamcord/android/KC_K$1;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_K;->h()V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_K;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_K;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_K$1;->a:Lcom/kamcord/android/KC_K;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_K$1;->a:Lcom/kamcord/android/KC_K;

    invoke-static {v0}, Lcom/kamcord/android/KC_K;->a(Lcom/kamcord/android/KC_K;)Lcom/kamcord/android/KC_K$KC_a;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/KC_K$KC_a;->e:Lcom/kamcord/android/KC_K$KC_a;

    if-ne v0, v1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_K$1;->a:Lcom/kamcord/android/KC_K;

    invoke-static {v0}, Lcom/kamcord/android/KC_K;->c(Lcom/kamcord/android/KC_K;)Landroid/media/MediaPlayer;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_K$1;->a:Lcom/kamcord/android/KC_K;

    invoke-static {v1}, Lcom/kamcord/android/KC_K;->b(Lcom/kamcord/android/KC_K;)Landroid/media/MediaPlayer;

    move-result-object v1

    invoke-virtual {v1}, Landroid/media/MediaPlayer;->getCurrentPosition()I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/media/MediaPlayer;->seekTo(I)V

    :cond_0
    return-void
.end method
