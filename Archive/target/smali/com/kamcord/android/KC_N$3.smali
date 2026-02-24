.class final Lcom/kamcord/android/KC_N$3;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_N;->a_(Ljava/lang/String;Z)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Z

.field private synthetic b:Lcom/kamcord/android/KC_N;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N;Z)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$3;->b:Lcom/kamcord/android/KC_N;

    iput-boolean p2, p0, Lcom/kamcord/android/KC_N$3;->a:Z

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 3

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/KC_N$3;->b:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->M:Landroid/app/Activity;

    const-string v1, "id"

    const-string v2, "mainlayout"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/app/Activity;->findViewById(I)Landroid/view/View;

    move-result-object v0

    check-cast v0, Landroid/view/ViewGroup;

    invoke-static {v0}, La/a/a/c/KC_a;->b(Landroid/view/ViewGroup;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$3;->b:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$3;->b:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->P:Landroid/widget/EditText;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setEnabled(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$3;->b:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->P:Landroid/widget/EditText;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setFocusable(Z)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$3;->b:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->P:Landroid/widget/EditText;

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Landroid/widget/EditText;->setFocusableInTouchMode(Z)V

    const/4 v0, 0x0

    :goto_0
    const/4 v1, 0x4

    if-ge v0, v1, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/KC_N$3;->b:Lcom/kamcord/android/KC_N;

    iget-object v1, v1, Lcom/kamcord/android/KC_N;->S:[Landroid/widget/LinearLayout;

    aget-object v1, v1, v0

    const/4 v2, 0x1

    invoke-virtual {v1, v2}, Landroid/widget/LinearLayout;->setEnabled(Z)V

    add-int/lit8 v0, v0, 0x1

    goto :goto_0

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_N$3;->b:Lcom/kamcord/android/KC_N;

    invoke-static {v0}, Lcom/kamcord/android/KC_Y;->b(Lcom/kamcord/android/KC_X;)Z

    iget-boolean v0, p0, Lcom/kamcord/android/KC_N$3;->a:Z

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/KC_N$3;->b:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    const-string v1, "string"

    const-string v2, "kamcordShare"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setText(I)V

    :goto_1
    return-void

    :cond_1
    iget-object v0, p0, Lcom/kamcord/android/KC_N$3;->b:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->O:Landroid/widget/Button;

    const-string v1, "string"

    const-string v2, "kamcordRetry"

    invoke-static {v1, v2}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v1

    invoke-virtual {v0, v1}, Landroid/widget/Button;->setText(I)V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_1

    :catch_0
    move-exception v0

    const-string v0, "Something went wrong when finishing uploading!"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    goto :goto_1
.end method
