.class final Lcom/kamcord/android/KC_w$7;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/widget/SeekBar$OnSeekBarChangeListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_w;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private a:Z

.field private synthetic b:Lcom/kamcord/android/KC_w;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_w;)V
    .locals 1

    iput-object p1, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/KC_w$7;->a:Z

    return-void
.end method


# virtual methods
.method public final onProgressChanged(Landroid/widget/SeekBar;IZ)V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->c(Lcom/kamcord/android/KC_w;)Lcom/kamcord/android/KC_E;

    move-result-object v0

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    if-eqz p3, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->c(Lcom/kamcord/android/KC_w;)Lcom/kamcord/android/KC_E;

    move-result-object v0

    invoke-interface {v0}, Lcom/kamcord/android/KC_E;->getDuration()I

    move-result v0

    iget-object v1, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    mul-int/2addr v0, p2

    div-int/lit16 v0, v0, 0x3e8

    invoke-static {v1, v0}, Lcom/kamcord/android/KC_w;->a(Lcom/kamcord/android/KC_w;I)I

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->f(Lcom/kamcord/android/KC_w;)Landroid/widget/TextView;

    move-result-object v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->f(Lcom/kamcord/android/KC_w;)Landroid/widget/TextView;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    iget-object v2, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v2}, Lcom/kamcord/android/KC_w;->g(Lcom/kamcord/android/KC_w;)I

    move-result v2

    invoke-static {v1, v2}, Lcom/kamcord/android/KC_w;->b(Lcom/kamcord/android/KC_w;I)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(Ljava/lang/CharSequence;)V

    goto :goto_0
.end method

.method public final onStartTrackingTouch(Landroid/widget/SeekBar;)V
    .locals 2

    const-string v0, "mSeekListener.onStartTrackingTouch()"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    const v1, 0x36ee80

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_w;->a(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    const/4 v1, 0x1

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_w;->a(Lcom/kamcord/android/KC_w;Z)Z

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    iget-object v1, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v1}, Lcom/kamcord/android/KC_w;->c(Lcom/kamcord/android/KC_w;)Lcom/kamcord/android/KC_E;

    move-result-object v1

    invoke-interface {v1}, Lcom/kamcord/android/KC_E;->getCurrentPosition()I

    move-result v1

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_w;->a(Lcom/kamcord/android/KC_w;I)I

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    iget-object v1, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v1}, Lcom/kamcord/android/KC_w;->c(Lcom/kamcord/android/KC_w;)Lcom/kamcord/android/KC_E;

    move-result-object v1

    invoke-interface {v1}, Lcom/kamcord/android/KC_E;->isPlaying()Z

    move-result v1

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_w;->b(Lcom/kamcord/android/KC_w;Z)Z

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->d(Lcom/kamcord/android/KC_w;)Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->c(Lcom/kamcord/android/KC_w;)Lcom/kamcord/android/KC_E;

    move-result-object v0

    invoke-interface {v0}, Lcom/kamcord/android/KC_E;->pause()V

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->e(Lcom/kamcord/android/KC_w;)Landroid/os/Handler;

    move-result-object v0

    const/4 v1, 0x2

    invoke-virtual {v0, v1}, Landroid/os/Handler;->removeMessages(I)V

    return-void
.end method

.method public final onStopTrackingTouch(Landroid/widget/SeekBar;)V
    .locals 2

    const-string v0, "mSeekListener.onStopTrackingTouch()"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    const/4 v1, 0x0

    invoke-static {v0, v1}, Lcom/kamcord/android/KC_w;->a(Lcom/kamcord/android/KC_w;Z)Z

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->c(Lcom/kamcord/android/KC_w;)Lcom/kamcord/android/KC_E;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v1}, Lcom/kamcord/android/KC_w;->g(Lcom/kamcord/android/KC_w;)I

    move-result v1

    invoke-interface {v0, v1}, Lcom/kamcord/android/KC_E;->seekTo(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->d(Lcom/kamcord/android/KC_w;)Z

    move-result v0

    if-eqz v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->c(Lcom/kamcord/android/KC_w;)Lcom/kamcord/android/KC_E;

    move-result-object v0

    invoke-interface {v0}, Lcom/kamcord/android/KC_E;->start()V

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->e()I

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_w;->g()V

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    const/16 v1, 0xfa0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_w;->a(I)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w$7;->b:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->e(Lcom/kamcord/android/KC_w;)Landroid/os/Handler;

    move-result-object v0

    const/4 v1, 0x2

    invoke-virtual {v0, v1}, Landroid/os/Handler;->sendEmptyMessage(I)Z

    return-void
.end method
