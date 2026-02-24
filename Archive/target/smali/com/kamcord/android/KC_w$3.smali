.class final Lcom/kamcord/android/KC_w$3;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnTouchListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_w;->a(Landroid/view/View;)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_w;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_w;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_w$3;->a:Lcom/kamcord/android/KC_w;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onTouch(Landroid/view/View;Landroid/view/MotionEvent;)Z
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_w$3;->a:Lcom/kamcord/android/KC_w;

    const/16 v1, 0xfa0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_w;->a(I)V

    const/4 v0, 0x1

    return v0
.end method
