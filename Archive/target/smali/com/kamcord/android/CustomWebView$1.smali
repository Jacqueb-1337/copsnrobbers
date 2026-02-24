.class final Lcom/kamcord/android/CustomWebView$1;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnTouchListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/CustomWebView;->b()V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/CustomWebView;


# direct methods
.method constructor <init>(Lcom/kamcord/android/CustomWebView;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/CustomWebView$1;->a:Lcom/kamcord/android/CustomWebView;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onTouch(Landroid/view/View;Landroid/view/MotionEvent;)Z
    .locals 4
    .annotation build Landroid/annotation/SuppressLint;
        value = {
            "ClickableViewAccessibility"
        }
    .end annotation

    invoke-virtual {p1}, Landroid/view/View;->isEnabled()Z

    move-result v0

    if-nez v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    invoke-virtual {p2}, Landroid/view/MotionEvent;->getAction()I

    move-result v0

    packed-switch v0, :pswitch_data_0

    :goto_1
    iget-object v0, p0, Lcom/kamcord/android/CustomWebView$1;->a:Lcom/kamcord/android/CustomWebView;

    invoke-static {v0}, Lcom/kamcord/android/CustomWebView;->b(Lcom/kamcord/android/CustomWebView;)Z

    move-result v0

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView$1;->a:Lcom/kamcord/android/CustomWebView;

    invoke-static {v0, p2}, Lcom/kamcord/android/CustomWebView;->a(Lcom/kamcord/android/CustomWebView;Landroid/view/MotionEvent;)V

    :cond_1
    const/4 v0, 0x0

    goto :goto_0

    :pswitch_0
    iget-object v0, p0, Lcom/kamcord/android/CustomWebView$1;->a:Lcom/kamcord/android/CustomWebView;

    invoke-static {v0}, Lcom/kamcord/android/CustomWebView;->a(Lcom/kamcord/android/CustomWebView;)Landroid/webkit/WebView;

    move-result-object v0

    invoke-virtual {v0}, Landroid/webkit/WebView;->getScrollY()I

    move-result v0

    iget-object v1, p0, Lcom/kamcord/android/CustomWebView$1;->a:Lcom/kamcord/android/CustomWebView;

    invoke-static {v1}, Lcom/kamcord/android/CustomWebView;->a(Lcom/kamcord/android/CustomWebView;)Landroid/webkit/WebView;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/android/CustomWebView$1;->a:Lcom/kamcord/android/CustomWebView;

    invoke-static {v2}, Lcom/kamcord/android/CustomWebView;->a(Lcom/kamcord/android/CustomWebView;)Landroid/webkit/WebView;

    move-result-object v2

    invoke-virtual {v2}, Landroid/webkit/WebView;->getScrollX()I

    move-result v2

    iget-object v3, p0, Lcom/kamcord/android/CustomWebView$1;->a:Lcom/kamcord/android/CustomWebView;

    invoke-static {v3}, Lcom/kamcord/android/CustomWebView;->a(Lcom/kamcord/android/CustomWebView;)Landroid/webkit/WebView;

    move-result-object v3

    invoke-virtual {v3}, Landroid/webkit/WebView;->getScrollY()I

    move-result v3

    add-int/lit8 v3, v3, 0x1

    invoke-virtual {v1, v2, v3}, Landroid/webkit/WebView;->scrollTo(II)V

    iget-object v1, p0, Lcom/kamcord/android/CustomWebView$1;->a:Lcom/kamcord/android/CustomWebView;

    invoke-static {v1}, Lcom/kamcord/android/CustomWebView;->a(Lcom/kamcord/android/CustomWebView;)Landroid/webkit/WebView;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/android/CustomWebView$1;->a:Lcom/kamcord/android/CustomWebView;

    invoke-static {v2}, Lcom/kamcord/android/CustomWebView;->a(Lcom/kamcord/android/CustomWebView;)Landroid/webkit/WebView;

    move-result-object v2

    invoke-virtual {v2}, Landroid/webkit/WebView;->getScrollX()I

    move-result v2

    invoke-virtual {v1, v2, v0}, Landroid/webkit/WebView;->scrollTo(II)V

    goto :goto_1

    nop

    :pswitch_data_0
    .packed-switch 0x0
        :pswitch_0
    .end packed-switch
.end method
