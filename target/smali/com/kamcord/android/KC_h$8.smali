.class final Lcom/kamcord/android/KC_h$8;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/content/DialogInterface$OnClickListener;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_h;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Ljava/lang/String;

.field private synthetic b:Ljava/lang/String;

.field private synthetic c:Ljava/lang/String;

.field private synthetic d:Lcom/kamcord/android/KC_h;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_h;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_h$8;->d:Lcom/kamcord/android/KC_h;

    iput-object p2, p0, Lcom/kamcord/android/KC_h$8;->a:Ljava/lang/String;

    iput-object p3, p0, Lcom/kamcord/android/KC_h$8;->b:Ljava/lang/String;

    iput-object p4, p0, Lcom/kamcord/android/KC_h$8;->c:Ljava/lang/String;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final onClick(Landroid/content/DialogInterface;I)V
    .locals 4

    invoke-static {}, La/a/a/c/KC_a;->a()Z

    move-result v0

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/KC_x;

    invoke-direct {v0}, Lcom/kamcord/android/KC_x;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/KC_h$8;->d:Lcom/kamcord/android/KC_h;

    invoke-static {v1}, Lcom/kamcord/android/KC_h;->d(Lcom/kamcord/android/KC_h;)Lcom/kamcord/android/KamcordActivity;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/KamcordActivity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Lcom/kamcord/android/KC_x;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    :goto_0
    return-void

    :cond_0
    iget-object v0, p0, Lcom/kamcord/android/KC_h$8;->d:Lcom/kamcord/android/KC_h;

    invoke-static {v0}, Lcom/kamcord/android/KC_h;->e(Lcom/kamcord/android/KC_h;)Landroid/view/View;

    move-result-object v0

    invoke-virtual {v0}, Landroid/view/View;->getParent()Landroid/view/ViewParent;

    move-result-object v0

    check-cast v0, Landroid/view/ViewGroup;

    invoke-static {v0}, La/a/a/c/KC_a;->a(Landroid/view/ViewGroup;)V

    new-instance v0, Lcom/kamcord/android/KC_h$KC_a;

    iget-object v1, p0, Lcom/kamcord/android/KC_h$8;->d:Lcom/kamcord/android/KC_h;

    invoke-direct {v0, v1}, Lcom/kamcord/android/KC_h$KC_a;-><init>(Lcom/kamcord/android/KC_h;)V

    const/4 v1, 0x3

    new-array v1, v1, [Ljava/lang/String;

    const/4 v2, 0x0

    iget-object v3, p0, Lcom/kamcord/android/KC_h$8;->a:Ljava/lang/String;

    aput-object v3, v1, v2

    const/4 v2, 0x1

    iget-object v3, p0, Lcom/kamcord/android/KC_h$8;->b:Ljava/lang/String;

    aput-object v3, v1, v2

    const/4 v2, 0x2

    iget-object v3, p0, Lcom/kamcord/android/KC_h$8;->c:Ljava/lang/String;

    aput-object v3, v1, v2

    invoke-virtual {v0, v1}, Lcom/kamcord/android/KC_h$KC_a;->execute([Ljava/lang/Object;)Landroid/os/AsyncTask;

    goto :goto_0
.end method
