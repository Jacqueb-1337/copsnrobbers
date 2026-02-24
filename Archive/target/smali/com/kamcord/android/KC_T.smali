.class public final Lcom/kamcord/android/KC_T;
.super Ljava/lang/Object;

# interfaces
.implements Lcom/kamcord/android/KC_r;


# instance fields
.field private a:La/a/a/a/KC_h;

.field private b:I

.field private c:Ljava/util/ArrayList;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/ArrayList",
            "<",
            "Ljava/util/Stack",
            "<",
            "La/a/a/a/KC_e;",
            ">;>;"
        }
    .end annotation
.end field

.field private d:Ljava/util/Set;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/Set",
            "<",
            "Ljava/lang/Class",
            "<*>;>;"
        }
    .end annotation
.end field


# direct methods
.method public constructor <init>(La/a/a/a/KC_h;I)V
    .locals 3

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/KC_T;->a:La/a/a/a/KC_h;

    invoke-direct {p0}, Lcom/kamcord/android/KC_T;->b()V

    new-instance v0, Ljava/util/ArrayList;

    invoke-direct {v0, p2}, Ljava/util/ArrayList;-><init>(I)V

    iput-object v0, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    const/4 v0, 0x0

    :goto_0
    if-ge v0, p2, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    new-instance v2, Ljava/util/Stack;

    invoke-direct {v2}, Ljava/util/Stack;-><init>()V

    invoke-virtual {v1, v0, v2}, Ljava/util/ArrayList;->add(ILjava/lang/Object;)V

    add-int/lit8 v0, v0, 0x1

    goto :goto_0

    :cond_0
    new-instance v0, Ljava/util/concurrent/ConcurrentHashMap;

    invoke-direct {v0}, Ljava/util/concurrent/ConcurrentHashMap;-><init>()V

    invoke-static {v0}, Ljava/util/Collections;->newSetFromMap(Ljava/util/Map;)Ljava/util/Set;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/KC_T;->d:Ljava/util/Set;

    return-void
.end method

.method private b()V
    .locals 2

    :goto_0
    iget-object v0, p0, Lcom/kamcord/android/KC_T;->a:La/a/a/a/KC_h;

    const v1, 0x7b78540

    invoke-virtual {v0, v1}, La/a/a/a/KC_h;->a(I)La/a/a/a/KC_e;

    move-result-object v0

    if-eqz v0, :cond_0

    iget-object v1, p0, Lcom/kamcord/android/KC_T;->a:La/a/a/a/KC_h;

    invoke-virtual {v1}, La/a/a/a/KC_h;->a()La/a/a/a/KC_m;

    move-result-object v1

    invoke-virtual {v1, v0}, La/a/a/a/KC_m;->a(La/a/a/a/KC_e;)La/a/a/a/KC_m;

    move-result-object v0

    invoke-virtual {v0}, La/a/a/a/KC_m;->a()I

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->a:La/a/a/a/KC_h;

    invoke-virtual {v0}, La/a/a/a/KC_h;->b()Z

    goto :goto_0

    :cond_0
    :goto_1
    iget-object v0, p0, Lcom/kamcord/android/KC_T;->a:La/a/a/a/KC_h;

    const v1, 0x6403276

    invoke-virtual {v0, v1}, La/a/a/a/KC_h;->a(I)La/a/a/a/KC_e;

    move-result-object v0

    if-eqz v0, :cond_1

    iget-object v1, p0, Lcom/kamcord/android/KC_T;->a:La/a/a/a/KC_h;

    invoke-virtual {v1}, La/a/a/a/KC_h;->a()La/a/a/a/KC_m;

    move-result-object v1

    invoke-virtual {v1, v0}, La/a/a/a/KC_m;->a(La/a/a/a/KC_e;)La/a/a/a/KC_m;

    move-result-object v0

    invoke-virtual {v0}, La/a/a/a/KC_m;->a()I

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->a:La/a/a/a/KC_h;

    invoke-virtual {v0}, La/a/a/a/KC_h;->b()Z

    goto :goto_1

    :cond_1
    return-void
.end method

.method private b(ILa/a/a/a/KC_e;I)V
    .locals 4

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "in TabFragmentManager.addToStack("

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, ", "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, ", "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p3}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, ")"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->a:La/a/a/a/KC_h;

    invoke-virtual {v0}, La/a/a/a/KC_h;->a()La/a/a/a/KC_m;

    move-result-object v1

    invoke-virtual {v1, p1, p2}, La/a/a/a/KC_m;->a(ILa/a/a/a/KC_e;)La/a/a/a/KC_m;

    move-result-object v0

    const/16 v2, 0x1001

    invoke-virtual {v0, v2}, La/a/a/a/KC_m;->a(I)La/a/a/a/KC_m;

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    invoke-virtual {v0, p3}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Stack;

    invoke-virtual {v0}, Ljava/util/Stack;->size()I

    move-result v0

    if-lez v0, :cond_0

    invoke-virtual {p0, p3}, Lcom/kamcord/android/KC_T;->a(I)La/a/a/a/KC_e;

    move-result-object v2

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    invoke-virtual {v0, p3}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Stack;

    invoke-virtual {v0}, Ljava/util/Stack;->size()I

    move-result v0

    const/16 v3, 0x32

    if-le v0, v3, :cond_1

    invoke-virtual {v1, v2}, La/a/a/a/KC_m;->a(La/a/a/a/KC_e;)La/a/a/a/KC_m;

    :cond_0
    :goto_0
    invoke-virtual {v1}, La/a/a/a/KC_m;->a()I

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    invoke-virtual {v0, p3}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Stack;

    invoke-virtual {v0, p2}, Ljava/util/Stack;->push(Ljava/lang/Object;)Ljava/lang/Object;

    return-void

    :cond_1
    invoke-virtual {v1, v2}, La/a/a/a/KC_m;->b(La/a/a/a/KC_e;)La/a/a/a/KC_m;

    goto :goto_0
.end method


# virtual methods
.method public final a(I)La/a/a/a/KC_e;
    .locals 1

    if-ltz p1, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    invoke-virtual {v0}, Ljava/util/ArrayList;->size()I

    move-result v0

    if-ge p1, v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    invoke-virtual {v0, p1}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Stack;

    invoke-virtual {v0}, Ljava/util/Stack;->size()I

    move-result v0

    if-lez v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    invoke-virtual {v0, p1}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Stack;

    invoke-virtual {v0}, Ljava/util/Stack;->peek()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_e;

    :goto_0
    return-object v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public final a(ILa/a/a/a/KC_e;I)V
    .locals 0

    invoke-direct {p0, p1, p2, p3}, Lcom/kamcord/android/KC_T;->b(ILa/a/a/a/KC_e;I)V

    return-void
.end method

.method public final a(La/a/a/a/KC_e;)V
    .locals 2

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    iget v1, p0, Lcom/kamcord/android/KC_T;->b:I

    invoke-virtual {v0, v1}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Stack;

    invoke-virtual {v0}, Ljava/util/Stack;->size()I

    move-result v0

    if-lez v0, :cond_0

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    iget v1, p0, Lcom/kamcord/android/KC_T;->b:I

    invoke-virtual {v0, v1}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Stack;

    invoke-virtual {v0}, Ljava/util/Stack;->peek()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_e;

    invoke-virtual {v0}, La/a/a/a/KC_e;->n()Landroid/view/View;

    move-result-object v0

    invoke-virtual {v0}, Landroid/view/View;->getParent()Landroid/view/ViewParent;

    move-result-object v0

    check-cast v0, Landroid/view/ViewGroup;

    invoke-virtual {v0}, Landroid/view/ViewGroup;->getId()I

    move-result v0

    iget v1, p0, Lcom/kamcord/android/KC_T;->b:I

    invoke-direct {p0, v0, p1, v1}, Lcom/kamcord/android/KC_T;->b(ILa/a/a/a/KC_e;I)V

    :cond_0
    return-void
.end method

.method public final a(Ljava/lang/Class;)V
    .locals 1
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/lang/Class",
            "<*>;)V"
        }
    .end annotation

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->d:Ljava/util/Set;

    invoke-interface {v0, p1}, Ljava/util/Set;->add(Ljava/lang/Object;)Z

    return-void
.end method

.method public final a()Z
    .locals 5

    const/4 v2, 0x1

    const-string v0, "in TabFragmentManager.back()"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    const/4 v1, 0x0

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v3, "  currentTabPosition: "

    invoke-direct {v0, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget v3, p0, Lcom/kamcord/android/KC_T;->b:I

    invoke-virtual {v0, v3}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    iget v3, p0, Lcom/kamcord/android/KC_T;->b:I

    invoke-virtual {v0, v3}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Stack;

    invoke-virtual {v0}, Ljava/util/Stack;->size()I

    move-result v0

    if-le v0, v2, :cond_2

    iget-object v0, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    iget v1, p0, Lcom/kamcord/android/KC_T;->b:I

    invoke-virtual {v0, v1}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/util/Stack;

    invoke-virtual {v0}, Ljava/util/Stack;->pop()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_e;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v3, "  removing fragment "

    invoke-direct {v1, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-object v1, p0, Lcom/kamcord/android/KC_T;->a:La/a/a/a/KC_h;

    invoke-virtual {v1}, La/a/a/a/KC_h;->a()La/a/a/a/KC_m;

    move-result-object v3

    iget-object v1, p0, Lcom/kamcord/android/KC_T;->c:Ljava/util/ArrayList;

    iget v4, p0, Lcom/kamcord/android/KC_T;->b:I

    invoke-virtual {v1, v4}, Ljava/util/ArrayList;->get(I)Ljava/lang/Object;

    move-result-object v1

    check-cast v1, Ljava/util/Stack;

    invoke-virtual {v1}, Ljava/util/Stack;->size()I

    move-result v1

    if-lez v1, :cond_0

    iget v1, p0, Lcom/kamcord/android/KC_T;->b:I

    invoke-virtual {p0, v1}, Lcom/kamcord/android/KC_T;->a(I)La/a/a/a/KC_e;

    move-result-object v1

    invoke-virtual {v3, v1}, La/a/a/a/KC_m;->c(La/a/a/a/KC_e;)La/a/a/a/KC_m;

    :cond_0
    invoke-virtual {v3, v0}, La/a/a/a/KC_m;->a(La/a/a/a/KC_e;)La/a/a/a/KC_m;

    move-result-object v0

    const/16 v1, 0x2002

    invoke-virtual {v0, v1}, La/a/a/a/KC_m;->a(I)La/a/a/a/KC_m;

    move-result-object v0

    invoke-virtual {v0}, La/a/a/a/KC_m;->a()I

    iget v0, p0, Lcom/kamcord/android/KC_T;->b:I

    invoke-virtual {p0, v0}, Lcom/kamcord/android/KC_T;->a(I)La/a/a/a/KC_e;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v0

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/android/KC_T;->d:Ljava/util/Set;

    invoke-interface {v1, v0}, Ljava/util/Set;->contains(Ljava/lang/Object;)Z

    move-result v1

    if-eqz v1, :cond_1

    const-string v1, "reload"

    const/4 v3, 0x0

    invoke-virtual {v0, v1, v3}, Ljava/lang/Class;->getMethod(Ljava/lang/String;[Ljava/lang/Class;)Ljava/lang/reflect/Method;
    :try_end_0
    .catch Ljava/lang/NoSuchMethodException; {:try_start_0 .. :try_end_0} :catch_1

    move-result-object v1

    if-eqz v1, :cond_1

    :try_start_1
    iget v3, p0, Lcom/kamcord/android/KC_T;->b:I

    invoke-virtual {p0, v3}, Lcom/kamcord/android/KC_T;->a(I)La/a/a/a/KC_e;

    move-result-object v3

    const/4 v4, 0x0

    invoke-virtual {v1, v3, v4}, Ljava/lang/reflect/Method;->invoke(Ljava/lang/Object;[Ljava/lang/Object;)Ljava/lang/Object;

    iget-object v1, p0, Lcom/kamcord/android/KC_T;->d:Ljava/util/Set;

    invoke-interface {v1, v0}, Ljava/util/Set;->remove(Ljava/lang/Object;)Z
    :try_end_1
    .catch Ljava/lang/Exception; {:try_start_1 .. :try_end_1} :catch_0
    .catch Ljava/lang/NoSuchMethodException; {:try_start_1 .. :try_end_1} :catch_1

    :cond_1
    :goto_0
    move v0, v2

    :goto_1
    return v0

    :catch_0
    move-exception v1

    :try_start_2
    new-instance v1, Ljava/lang/StringBuilder;

    const-string v3, "Error invoking the reload method of class "

    invoke-direct {v1, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I
    :try_end_2
    .catch Ljava/lang/NoSuchMethodException; {:try_start_2 .. :try_end_2} :catch_1

    goto :goto_0

    :catch_1
    move-exception v1

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v3, "Class "

    invoke-direct {v1, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, " has no reload method."

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->b(Ljava/lang/String;)I

    goto :goto_0

    :cond_2
    move v0, v1

    goto :goto_1
.end method

.method public final onTabReselected$7f50f85c(La/a/a/c/KC_a;)V
    .locals 0

    return-void
.end method

.method public final onTabSelected$7f50f85c(La/a/a/c/KC_a;)V
    .locals 1

    invoke-virtual {p1}, La/a/a/c/KC_a;->e()I

    move-result v0

    iput v0, p0, Lcom/kamcord/android/KC_T;->b:I

    return-void
.end method

.method public final onTabUnselected$7f50f85c(La/a/a/c/KC_a;)V
    .locals 0

    return-void
.end method
