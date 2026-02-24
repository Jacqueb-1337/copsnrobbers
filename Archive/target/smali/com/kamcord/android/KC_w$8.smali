.class final Lcom/kamcord/android/KC_w$8;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/KC_w;
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

    iput-object p1, p0, Lcom/kamcord/android/KC_w$8;->a:Lcom/kamcord/android/KC_w;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_w$8;->a:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->h(Lcom/kamcord/android/KC_w;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w$8;->a:Lcom/kamcord/android/KC_w;

    invoke-static {v0}, Lcom/kamcord/android/KC_w;->i(Lcom/kamcord/android/KC_w;)Landroid/view/View$OnClickListener;

    move-result-object v0

    invoke-interface {v0, p1}, Landroid/view/View$OnClickListener;->onClick(Landroid/view/View;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_w$8;->a:Lcom/kamcord/android/KC_w;

    const/16 v1, 0xfa0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_w;->a(I)V

    return-void
.end method
