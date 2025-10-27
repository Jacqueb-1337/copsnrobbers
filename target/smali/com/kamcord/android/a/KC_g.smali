.class final Lcom/kamcord/android/a/KC_g;
.super Ljava/lang/Object;


# instance fields
.field private a:Ljava/util/LinkedList;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/LinkedList",
            "<",
            "Lcom/kamcord/android/a/KC_h;",
            ">;"
        }
    .end annotation
.end field


# direct methods
.method constructor <init>()V
    .locals 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    new-instance v0, Ljava/util/LinkedList;

    invoke-direct {v0}, Ljava/util/LinkedList;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/a/KC_g;->a:Ljava/util/LinkedList;

    return-void
.end method


# virtual methods
.method final a()V
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/a/KC_g;->a:Ljava/util/LinkedList;

    invoke-virtual {v0}, Ljava/util/LinkedList;->clear()V

    return-void
.end method

.method final a(Ljava/lang/String;)V
    .locals 5

    iget-object v0, p0, Lcom/kamcord/android/a/KC_g;->a:Ljava/util/LinkedList;

    new-instance v1, Lcom/kamcord/android/a/KC_h;

    const-string v2, ""

    const/4 v3, -0x1

    const/4 v4, 0x1

    invoke-direct {v1, v2, p1, v3, v4}, Lcom/kamcord/android/a/KC_h;-><init>(Ljava/lang/String;Ljava/lang/String;IZ)V

    invoke-virtual {v0, v1}, Ljava/util/LinkedList;->addFirst(Ljava/lang/Object;)V

    return-void
.end method

.method final a(Ljava/lang/String;I)V
    .locals 4

    iget-object v0, p0, Lcom/kamcord/android/a/KC_g;->a:Ljava/util/LinkedList;

    new-instance v1, Lcom/kamcord/android/a/KC_h;

    const-string v2, ""

    const/4 v3, 0x1

    invoke-direct {v1, v2, p1, p2, v3}, Lcom/kamcord/android/a/KC_h;-><init>(Ljava/lang/String;Ljava/lang/String;IZ)V

    invoke-virtual {v0, v1}, Ljava/util/LinkedList;->addFirst(Ljava/lang/Object;)V

    return-void
.end method

.method final b()Lcom/kamcord/android/a/KC_h;
    .locals 6

    sget-object v0, Landroid/os/Build;->BOARD:Ljava/lang/String;

    sget-object v1, Ljava/util/Locale;->ENGLISH:Ljava/util/Locale;

    invoke-virtual {v0, v1}, Ljava/lang/String;->toLowerCase(Ljava/util/Locale;)Ljava/lang/String;

    move-result-object v1

    sget-object v2, Landroid/os/Build;->DEVICE:Ljava/lang/String;

    sget v3, Landroid/os/Build$VERSION;->SDK_INT:I

    iget-object v0, p0, Lcom/kamcord/android/a/KC_g;->a:Ljava/util/LinkedList;

    invoke-virtual {v0}, Ljava/util/LinkedList;->iterator()Ljava/util/Iterator;

    move-result-object v4

    :cond_0
    invoke-interface {v4}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_3

    invoke-interface {v4}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/a/KC_h;

    iget-object v5, v0, Lcom/kamcord/android/a/KC_h;->a:Ljava/lang/String;

    invoke-virtual {v1, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v5

    if-nez v5, :cond_1

    iget-object v5, v0, Lcom/kamcord/android/a/KC_h;->b:Ljava/lang/String;

    invoke-virtual {v2, v5}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v5

    if-eqz v5, :cond_0

    :cond_1
    iget v5, v0, Lcom/kamcord/android/a/KC_h;->c:I

    if-ltz v5, :cond_2

    iget v5, v0, Lcom/kamcord/android/a/KC_h;->c:I

    if-ne v3, v5, :cond_0

    :cond_2
    :goto_0
    return-object v0

    :cond_3
    const/4 v0, 0x0

    goto :goto_0
.end method

.method final b(Ljava/lang/String;)V
    .locals 5

    iget-object v0, p0, Lcom/kamcord/android/a/KC_g;->a:Ljava/util/LinkedList;

    new-instance v1, Lcom/kamcord/android/a/KC_h;

    const-string v2, ""

    const/4 v3, -0x1

    const/4 v4, 0x0

    invoke-direct {v1, v2, p1, v3, v4}, Lcom/kamcord/android/a/KC_h;-><init>(Ljava/lang/String;Ljava/lang/String;IZ)V

    invoke-virtual {v0, v1}, Ljava/util/LinkedList;->addFirst(Ljava/lang/Object;)V

    return-void
.end method

.method final b(Ljava/lang/String;I)V
    .locals 4

    iget-object v0, p0, Lcom/kamcord/android/a/KC_g;->a:Ljava/util/LinkedList;

    new-instance v1, Lcom/kamcord/android/a/KC_h;

    const-string v2, ""

    const/4 v3, 0x0

    invoke-direct {v1, v2, p1, p2, v3}, Lcom/kamcord/android/a/KC_h;-><init>(Ljava/lang/String;Ljava/lang/String;IZ)V

    invoke-virtual {v0, v1}, Ljava/util/LinkedList;->addFirst(Ljava/lang/Object;)V

    return-void
.end method

.method final c(Ljava/lang/String;)V
    .locals 5

    iget-object v0, p0, Lcom/kamcord/android/a/KC_g;->a:Ljava/util/LinkedList;

    new-instance v1, Lcom/kamcord/android/a/KC_h;

    const-string v2, ""

    const/4 v3, -0x1

    const/4 v4, 0x1

    invoke-direct {v1, p1, v2, v3, v4}, Lcom/kamcord/android/a/KC_h;-><init>(Ljava/lang/String;Ljava/lang/String;IZ)V

    invoke-virtual {v0, v1}, Ljava/util/LinkedList;->addFirst(Ljava/lang/Object;)V

    return-void
.end method

.method final c(Ljava/lang/String;I)V
    .locals 4

    iget-object v0, p0, Lcom/kamcord/android/a/KC_g;->a:Ljava/util/LinkedList;

    new-instance v1, Lcom/kamcord/android/a/KC_h;

    const-string v2, ""

    const/4 v3, 0x1

    invoke-direct {v1, p1, v2, p2, v3}, Lcom/kamcord/android/a/KC_h;-><init>(Ljava/lang/String;Ljava/lang/String;IZ)V

    invoke-virtual {v0, v1}, Ljava/util/LinkedList;->addFirst(Ljava/lang/Object;)V

    return-void
.end method
