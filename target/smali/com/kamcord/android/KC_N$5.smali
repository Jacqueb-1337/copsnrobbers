.class final Lcom/kamcord/android/KC_N$5;
.super Ljava/lang/Object;

# interfaces
.implements Ljava/lang/Runnable;


# annotations
.annotation system Ldalvik/annotation/EnclosingMethod;
    value = Lcom/kamcord/android/KC_N;->a(Ljava/lang/String;Z)V
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x0
    name = null
.end annotation


# instance fields
.field private synthetic a:Z

.field private synthetic b:Ljava/lang/String;

.field private synthetic c:Lcom/kamcord/android/KC_N;


# direct methods
.method constructor <init>(Lcom/kamcord/android/KC_N;ZLjava/lang/String;)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/KC_N$5;->c:Lcom/kamcord/android/KC_N;

    iput-boolean p2, p0, Lcom/kamcord/android/KC_N$5;->a:Z

    iput-object p3, p0, Lcom/kamcord/android/KC_N$5;->b:Ljava/lang/String;

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final run()V
    .locals 4

    const/4 v0, 0x0

    :try_start_0
    iget-boolean v1, p0, Lcom/kamcord/android/KC_N$5;->a:Z

    if-nez v1, :cond_0

    const/4 v1, -0x1

    :goto_0
    const/4 v2, 0x4

    if-ge v0, v2, :cond_3

    iget-object v2, p0, Lcom/kamcord/android/KC_N$5;->c:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    aget-object v2, v2, v0

    if-eqz v2, :cond_1

    iget-object v2, p0, Lcom/kamcord/android/KC_N$5;->c:Lcom/kamcord/android/KC_N;

    iget-object v2, v2, Lcom/kamcord/android/KC_N;->T:[Lcom/kamcord/android/b/KC_e;

    aget-object v2, v2, v0

    invoke-virtual {v2}, Lcom/kamcord/android/b/KC_e;->d()Ljava/lang/String;

    move-result-object v2

    iget-object v3, p0, Lcom/kamcord/android/KC_N$5;->b:Ljava/lang/String;

    invoke-virtual {v2, v3}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_1

    :goto_1
    if-gez v0, :cond_2

    :cond_0
    :goto_2
    return-void

    :cond_1
    add-int/lit8 v0, v0, 0x1

    goto :goto_0

    :cond_2
    iget-object v1, p0, Lcom/kamcord/android/KC_N$5;->c:Lcom/kamcord/android/KC_N;

    const/4 v2, 0x0

    invoke-static {v1, v0, v2}, Lcom/kamcord/android/KC_N;->a(Lcom/kamcord/android/KC_N;IZ)V
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    goto :goto_2

    :catch_0
    move-exception v0

    const-string v0, "Something went wrong during an authentication change!"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    goto :goto_2

    :cond_3
    move v0, v1

    goto :goto_1
.end method
