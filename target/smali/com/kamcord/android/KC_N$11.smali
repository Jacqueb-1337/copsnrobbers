.class final Lcom/kamcord/android/KC_N$11;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_N;->a(Lcom/kamcord/android/core/KC_H;)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field final synthetic a:Lcom/kamcord/android/KC_N;

.field private synthetic b:Lcom/kamcord/android/core/KC_H;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N;Lcom/kamcord/android/core/KC_H;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$11;->a:Lcom/kamcord/android/KC_N;

    iput-object p2, p0, Lcom/kamcord/android/KC_N$11;->b:Lcom/kamcord/android/core/KC_H;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 3

    iget-object v0, p0, Lcom/kamcord/android/KC_N$11;->b:Lcom/kamcord/android/core/KC_H;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v0, v0, Lcom/kamcord/android/core/KC_H;->b:Ljava/lang/String;

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "/thumbnail.jpg"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Landroid/graphics/BitmapFactory;->decodeFile(Ljava/lang/String;)Landroid/graphics/Bitmap;

    move-result-object v0

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/KC_l;

    invoke-direct {v0}, Lcom/kamcord/android/KC_l;-><init>()V

    iget-object v1, p0, Lcom/kamcord/android/KC_N$11;->a:Lcom/kamcord/android/KC_N;

    iget-object v1, v1, Lcom/kamcord/android/KC_N;->M:Landroid/app/Activity;

    invoke-virtual {v1}, Landroid/app/Activity;->getFragmentManager()Landroid/app/FragmentManager;

    move-result-object v1

    const-string v2, "InvalidVideoDialogFragment"

    invoke-virtual {v0, v1, v2}, Landroid/app/DialogFragment;->show(Landroid/app/FragmentManager;Ljava/lang/String;)V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$11;->a:Lcom/kamcord/android/KC_N;

    invoke-virtual {v0}, Lcom/kamcord/android/KC_N;->h()La/a/a/a/KC_f;

    move-result-object v0

    invoke-virtual {v0}, La/a/a/a/KC_f;->finish()V

    :goto_0
    return-void

    :cond_0
    iget-object v1, p0, Lcom/kamcord/android/KC_N$11;->a:Lcom/kamcord/android/KC_N;

    invoke-virtual {v1}, Lcom/kamcord/android/KC_N;->h()La/a/a/a/KC_f;

    move-result-object v1

    new-instance v2, Lcom/kamcord/android/KC_N$11$1;

    invoke-direct {v2, p0, v0}, Lcom/kamcord/android/KC_N$11$1;-><init>(Lcom/kamcord/android/KC_N$11;Landroid/graphics/Bitmap;)V

    invoke-virtual {v1, v2}, La/a/a/a/KC_f;->runOnUiThread(Ljava/lang/Runnable;)V

    goto :goto_0
.end method
