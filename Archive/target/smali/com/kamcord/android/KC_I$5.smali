.class final Lcom/kamcord/android/KC_I$5;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_I;->a(Ljava/lang/String;Z)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field final synthetic a:I

.field final synthetic b:Lcom/kamcord/android/KC_I;

.field private synthetic c:Z


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_I;ZI)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    iput-boolean p2, p0, Lcom/kamcord/android/KC_I$5;->c:Z

    iput p3, p0, Lcom/kamcord/android/KC_I$5;->a:I

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 2

    iget-boolean v0, p0, Lcom/kamcord/android/KC_I$5;->c:Z

    if-nez v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->b(Lcom/kamcord/android/KC_I;)Landroid/view/View;

    move-result-object v0

    invoke-virtual {v0}, Landroid/view/View;->getParent()Landroid/view/ViewParent;

    move-result-object v0

    check-cast v0, Landroid/view/ViewGroup;

    invoke-static {v0}, La/a/a/c/KC_a;->b(Landroid/view/ViewGroup;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->c(Lcom/kamcord/android/KC_I;)Ljava/util/List;

    move-result-object v0

    iget v1, p0, Lcom/kamcord/android/KC_I$5;->a:I

    invoke-interface {v0, v1}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/widget/TextView;

    const-string v1, "kamcordNoAccountLinked"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/widget/TextView;->setText(Ljava/lang/CharSequence;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->d(Lcom/kamcord/android/KC_I;)Ljava/util/List;

    move-result-object v0

    iget v1, p0, Lcom/kamcord/android/KC_I$5;->a:I

    invoke-interface {v0, v1}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/widget/ToggleButton;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setChecked(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->d(Lcom/kamcord/android/KC_I;)Ljava/util/List;

    move-result-object v0

    iget v1, p0, Lcom/kamcord/android/KC_I$5;->a:I

    invoke-interface {v0, v1}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/widget/ToggleButton;

    const-string v1, "kamcordSignIn"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setText(Ljava/lang/CharSequence;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->d(Lcom/kamcord/android/KC_I;)Ljava/util/List;

    move-result-object v0

    iget v1, p0, Lcom/kamcord/android/KC_I$5;->a:I

    invoke-interface {v0, v1}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/widget/ToggleButton;

    new-instance v1, Lcom/kamcord/android/KC_I$5$1;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_I$5$1;-><init>(Lcom/kamcord/android/KC_I$5;)V

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    :goto_0
    return-void

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->d(Lcom/kamcord/android/KC_I;)Ljava/util/List;

    move-result-object v0

    iget v1, p0, Lcom/kamcord/android/KC_I$5;->a:I

    invoke-interface {v0, v1}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/widget/ToggleButton;

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setChecked(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->d(Lcom/kamcord/android/KC_I;)Ljava/util/List;

    move-result-object v0

    iget v1, p0, Lcom/kamcord/android/KC_I$5;->a:I

    invoke-interface {v0, v1}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/widget/ToggleButton;

    const-string v1, "kamcordSignOut"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord;->getString(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setText(Ljava/lang/CharSequence;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_I$5;->b:Lcom/kamcord/android/KC_I;

    invoke-static {v0}, Lcom/kamcord/android/KC_I;->d(Lcom/kamcord/android/KC_I;)Ljava/util/List;

    move-result-object v0

    iget v1, p0, Lcom/kamcord/android/KC_I$5;->a:I

    invoke-interface {v0, v1}, Ljava/util/List;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Landroid/widget/ToggleButton;

    new-instance v1, Lcom/kamcord/android/KC_I$5$2;

    invoke-direct {v1, p0}, Lcom/kamcord/android/KC_I$5$2;-><init>(Lcom/kamcord/android/KC_I$5;)V

    invoke-virtual {v0, v1}, Landroid/widget/ToggleButton;->setOnClickListener(Landroid/view/View$OnClickListener;)V

    goto :goto_0
.end method
