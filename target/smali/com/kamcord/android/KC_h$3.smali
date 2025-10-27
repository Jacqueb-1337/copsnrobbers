.class final Lcom/kamcord/android/KC_h$3;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnFocusChangeListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_h;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_h;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_h;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_h$3;->a:Lcom/kamcord/android/KC_h;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onFocusChange(Landroid/view/View;Z)V
    .locals 4

    const/4 v3, 0x1

    if-nez p2, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_h$3;->a:Lcom/kamcord/android/KC_h;

    invoke-static {v0}, Lcom/kamcord/android/KC_h;->c(Lcom/kamcord/android/KC_h;)Landroid/widget/EditText;

    move-result-object v0

    invoke-virtual {v0}, Landroid/widget/EditText;->getText()Landroid/text/Editable;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/Object;->toString()Ljava/lang/String;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_h$3;->a:Lcom/kamcord/android/KC_h;

    invoke-virtual {v1, v0}, Lcom/kamcord/android/KC_h;->c(Ljava/lang/String;)Z

    move-result v1

    if-eqz v1, :cond_0

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v1

    if-eqz v1, :cond_0

    new-instance v1, Lcom/kamcord/android/KC_h$KC_c;

    iget-object v2, p0, Lcom/kamcord/android/KC_h$3;->a:Lcom/kamcord/android/KC_h;

    invoke-direct {v1, v2, v3}, Lcom/kamcord/android/KC_h$KC_c;-><init>(Lcom/kamcord/android/KC_h;I)V

    new-array v2, v3, [Ljava/lang/String;

    const/4 v3, 0x0

    aput-object v0, v2, v3

    invoke-virtual {v1, v2}, Lcom/kamcord/android/KC_h$KC_c;->execute([Ljava/lang/Object;)Landroid/os/AsyncTask;

    :cond_0
    return-void
.end method
