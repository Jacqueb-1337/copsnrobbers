.class final Lcom/kamcord/android/KC_N$12;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnTouchListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_N;->a(Lcom/kamcord/android/core/KC_H;)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_N;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$12;->a:Lcom/kamcord/android/KC_N;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onTouch(Landroid/view/View;Landroid/view/MotionEvent;)Z
    .locals 6
    .annotation build Landroid/annotation/SuppressLint;
        value = {
            "ClickableViewAccessibility"
        }
    .end annotation

    const/16 v5, 0xff

    const/4 v4, 0x0

    const/4 v3, 0x0

    invoke-virtual {p2}, Landroid/view/MotionEvent;->getX()F

    move-result v0

    invoke-virtual {p2}, Landroid/view/MotionEvent;->getY()F

    move-result v1

    cmpg-float v2, v0, v3

    if-ltz v2, :cond_0

    invoke-virtual {p1}, Landroid/view/View;->getWidth()I

    move-result v2

    int-to-float v2, v2

    cmpl-float v0, v0, v2

    if-gtz v0, :cond_0

    cmpg-float v0, v1, v3

    if-ltz v0, :cond_0

    invoke-virtual {p1}, Landroid/view/View;->getHeight()I

    move-result v0

    int-to-float v0, v0

    cmpl-float v0, v1, v0

    if-lez v0, :cond_1

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_N$12;->a:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->N:Landroid/widget/ImageButton;

    invoke-virtual {v0, v5}, Landroid/widget/ImageButton;->setImageAlpha(I)V

    :goto_0
    return v4

    :cond_1
    invoke-virtual {p2}, Landroid/view/MotionEvent;->getAction()I

    move-result v0

    packed-switch v0, :pswitch_data_0

    :pswitch_0
    goto :goto_0

    :pswitch_1
    iget-object v0, p0, Lcom/kamcord/android/KC_N$12;->a:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->N:Landroid/widget/ImageButton;

    const/16 v1, 0x80

    invoke-virtual {v0, v1}, Landroid/widget/ImageButton;->setImageAlpha(I)V

    goto :goto_0

    :pswitch_2
    iget-object v0, p0, Lcom/kamcord/android/KC_N$12;->a:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->N:Landroid/widget/ImageButton;

    invoke-virtual {v0, v5}, Landroid/widget/ImageButton;->setImageAlpha(I)V

    goto :goto_0

    :pswitch_data_0
    .packed-switch 0x0
        :pswitch_1
        :pswitch_2
        :pswitch_0
        :pswitch_2
    .end packed-switch
.end method
