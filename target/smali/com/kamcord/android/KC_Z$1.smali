.class final Lcom/kamcord/android/KC_Z$1;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnTouchListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_Z;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_Z;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_Z;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_Z$1;->a:Lcom/kamcord/android/KC_Z;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onTouch(Landroid/view/View;Landroid/view/MotionEvent;)Z
    .locals 1
    .annotation build Landroid/annotation/SuppressLint;
        value = {
            "ClickableViewAccessibility"
        }
    .end annotation

    invoke-virtual {p2}, Landroid/view/MotionEvent;->getAction()I

    move-result v0

    if-nez v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_Z$1;->a:Lcom/kamcord/android/KC_Z;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_Z;->C()V

    :cond_0
    const/4 v0, 0x1

    return v0
.end method
