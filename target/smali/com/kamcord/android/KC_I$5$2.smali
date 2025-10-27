.class final Lcom/kamcord/android/KC_I$5$2;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/view/View$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_I$5;->run()V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Lcom/kamcord/android/KC_I$5;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_I$5;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_I$5$2;->a:Lcom/kamcord/android/KC_I$5;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/view/View;)V
    .locals 4

    new-instance v1, Lcom/kamcord/android/KC_I$KC_a;

    iget-object v0, p0, Lcom/kamcord/android/KC_I$5$2;->a:Lcom/kamcord/android/KC_I$5;

    iget-object v2, v0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    iget-object v0, p0, Lcom/kamcord/android/KC_I$5$2;->a:Lcom/kamcord/android/KC_I$5;

    iget-object v0, v0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->e(Lcom/kamcord/android/KC_I;)Ljava/util/List;

    move-result-object v0

    iget-object v3, p0, Lcom/kamcord/android/KC_I$5$2;->a:Lcom/kamcord/android/KC_I$5;

    iget v3, v3, Lcom/kamcord/android/KC_I$5;->a:I

    invoke-interface {v0, v3}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/b/KC_e;

    invoke-direct {v1, v2, v0}, Lcom/kamcord/android/KC_I$KC_a;-><init>(Lcom/kamcord/android/KC_I;Lcom/kamcord/android/b/KC_e;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I$5$2;->a:Lcom/kamcord/android/KC_I$5;

    iget-object v0, v0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->a(Lcom/kamcord/android/KC_I;)Lcom/kamcord/android/KamcordActivity;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/KamcordActivity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v0

    const/4 v2, 0x0

    invoke-virtual {v1, v0, v2}, Landroid/app/DialogFragment;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    return-void
.end method
