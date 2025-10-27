.class final Lcom/kamcord/android/KC_P$2;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_P;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_P;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_P;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_P$2;->a:Lcom/kamcord/android/KC_P;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/KC_P$2;->a:Lcom/kamcord/android/KC_P;

    invoke-static {v0}, Lcom/kamcord/android/KC_P;->a(Lcom/kamcord/android/KC_P;)Landroid/widget/EditText;

    move-result-object v0

    invoke-virtual {v0}, Landroid/widget/EditText;->getText()Landroid/text/Editable;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/Object;->toString()Ljava/lang/String;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/KC_P$2;->a:Lcom/kamcord/android/KC_P;

    invoke-static {v1}, Lcom/kamcord/android/KC_P;->b(Lcom/kamcord/android/KC_P;)Landroid/widget/EditText;

    move-result-object v1

    invoke-virtual {v1}, Landroid/widget/EditText;->getText()Landroid/text/Editable;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/Object;->toString()Ljava/lang/String;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/android/KC_P$2;->a:Lcom/kamcord/android/KC_P;

    invoke-virtual {v2, v0, v1}, Lcom/kamcord/android/KC_P;->a(Ljava/lang/String;Ljava/lang/String;)V

    return-void
.end method
