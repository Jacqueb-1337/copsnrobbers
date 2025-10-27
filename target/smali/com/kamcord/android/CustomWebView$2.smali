.class final Lcom/kamcord/android/CustomWebView$2;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/ViewTreeObserver$OnGlobalLayoutListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/CustomWebView;->a()V
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

    iput-object p1, p0, Lcom/kamcord/android/CustomWebView$2;->a:Lcom/kamcord/android/CustomWebView;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onGlobalLayout()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/CustomWebView$2;->a:Lcom/kamcord/android/CustomWebView;

    invoke-static {v0}, Lcom/kamcord/android/CustomWebView;->c(Lcom/kamcord/android/CustomWebView;)V

    return-void
.end method
