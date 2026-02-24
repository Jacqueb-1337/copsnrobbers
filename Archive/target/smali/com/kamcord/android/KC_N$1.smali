.class final Lcom/kamcord/android/KC_N$1;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_N;->a(IZ)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:I

.field private synthetic b:Z

.field private synthetic c:Lcom/kamcord/android/KC_N;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N;IZ)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$1;->c:Lcom/kamcord/android/KC_N;

    iput p2, p0, Lcom/kamcord/android/KC_N$1;->a:I

    iput-boolean p3, p0, Lcom/kamcord/android/KC_N$1;->b:Z

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 4

    iget-object v0, p0, Lcom/kamcord/android/KC_N$1;->c:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    iget v1, p0, Lcom/kamcord/android/KC_N$1;->a:I

    aget-object v0, v0, v1

    if-nez v0, :cond_0

    :goto_0
    return-void

    :cond_0
    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "shareOn"

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v2, p0, Lcom/kamcord/android/KC_N$1;->c:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    iget v3, p0, Lcom/kamcord/android/KC_N$1;->a:I

    aget-object v2, v2, v3

    invoke-virtual {v2}, Lcom/kamcord/android/b/KC_e;->d()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    iget-boolean v2, p0, Lcom/kamcord/android/KC_N$1;->b:Z

    invoke-interface {v0, v1, v2}, Landroid/content/SharedPreferences$Editor;->putBoolean(Ljava/lang/String;Z)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences$Editor;->apply()V

    iget-object v0, p0, Lcom/kamcord/android/KC_N$1;->c:Lcom/kamcord/android/KC_N;

    iget-object v0, v0, Lcom/kamcord/android/KC_N;->S:[Landroid/widget/LinearLayout;

    iget v1, p0, Lcom/kamcord/android/KC_N$1;->a:I

    aget-object v0, v0, v1

    iget-boolean v1, p0, Lcom/kamcord/android/KC_N$1;->b:Z

    invoke-virtual {v0, v1}, Landroid/widget/LinearLayout;->setSelected(Z)V

    goto :goto_0
.end method
