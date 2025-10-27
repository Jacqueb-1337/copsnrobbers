.class public La/a/a/a/KC_e;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements Landroid/content/ComponentCallbacks;
.implements Landroid/view/View$OnCreateContextMenuListener;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/a/KC_e$KC_a;
    }
.end annotation


# static fields
.field private static final M:La/a/a/d/KC_e;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "La/a/a/d/KC_e",
            "<",
            "Ljava/lang/String;",
            "Ljava/lang/Class",
            "<*>;>;"
        }
    .end annotation
.end field


# instance fields
.field A:Z

.field B:Z

.field C:Z

.field D:Z

.field E:Z

.field F:I

.field G:Landroid/view/ViewGroup;

.field H:Landroid/view/View;

.field I:Landroid/view/View;

.field J:Z

.field K:Z

.field L:La/a/a/a/KC_o;

.field private N:Z

.field private O:Z

.field a:I

.field b:Landroid/view/View;

.field c:I

.field d:Landroid/os/Bundle;

.field e:Landroid/util/SparseArray;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Landroid/util/SparseArray",
            "<",
            "Landroid/os/Parcelable;",
            ">;"
        }
    .end annotation
.end field

.field f:I

.field g:Ljava/lang/String;

.field h:Landroid/os/Bundle;

.field i:La/a/a/a/KC_e;

.field j:I

.field k:I

.field l:Z

.field m:Z

.field n:Z

.field o:Z

.field p:Z

.field q:Z

.field r:I

.field s:La/a/a/a/KC_i;

.field t:La/a/a/a/KC_f;

.field u:La/a/a/a/KC_i;

.field v:La/a/a/a/KC_e;

.field w:I

.field x:I

.field y:Ljava/lang/String;

.field z:Z


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 166
    new-instance v0, La/a/a/d/KC_e;

    invoke-direct {v0}, La/a/a/d/KC_e;-><init>()V

    sput-object v0, La/a/a/a/KC_e;->M:La/a/a/d/KC_e;

    return-void
.end method

.method public constructor <init>()V
    .locals 3

    .prologue
    const/4 v2, 0x1

    const/4 v1, -0x1

    .line 371
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 176
    const/4 v0, 0x0

    iput v0, p0, La/a/a/a/KC_e;->a:I

    .line 192
    iput v1, p0, La/a/a/a/KC_e;->f:I

    .line 204
    iput v1, p0, La/a/a/a/KC_e;->j:I

    .line 275
    iput-boolean v2, p0, La/a/a/a/KC_e;->D:Z

    .line 297
    iput-boolean v2, p0, La/a/a/a/KC_e;->K:Z

    .line 372
    return-void
.end method

.method public static a(Landroid/content/Context;Ljava/lang/String;)La/a/a/a/KC_e;
    .locals 1

    .prologue
    .line 379
    const/4 v0, 0x0

    invoke-static {p0, p1, v0}, La/a/a/a/KC_e;->a(Landroid/content/Context;Ljava/lang/String;Landroid/os/Bundle;)La/a/a/a/KC_e;

    move-result-object v0

    return-object v0
.end method

.method public static a(Landroid/content/Context;Ljava/lang/String;Landroid/os/Bundle;)La/a/a/a/KC_e;
    .locals 4

    .prologue
    .line 398
    :try_start_0
    sget-object v0, La/a/a/a/KC_e;->M:La/a/a/d/KC_e;

    invoke-virtual {v0, p1}, La/a/a/d/KC_e;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/lang/Class;

    .line 399
    if-nez v0, :cond_0

    .line 401
    invoke-virtual {p0}, Landroid/content/Context;->getClassLoader()Ljava/lang/ClassLoader;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/ClassLoader;->loadClass(Ljava/lang/String;)Ljava/lang/Class;

    move-result-object v0

    .line 402
    sget-object v1, La/a/a/a/KC_e;->M:La/a/a/d/KC_e;

    invoke-virtual {v1, p1, v0}, La/a/a/d/KC_e;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    .line 404
    :cond_0
    invoke-virtual {v0}, Ljava/lang/Class;->newInstance()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_e;

    .line 405
    if-eqz p2, :cond_1

    .line 406
    invoke-virtual {v0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/Class;->getClassLoader()Ljava/lang/ClassLoader;

    move-result-object v1

    invoke-virtual {p2, v1}, Landroid/os/Bundle;->setClassLoader(Ljava/lang/ClassLoader;)V

    .line 407
    iput-object p2, v0, La/a/a/a/KC_e;->h:Landroid/os/Bundle;
    :try_end_0
    .catch Ljava/lang/ClassNotFoundException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Ljava/lang/InstantiationException; {:try_start_0 .. :try_end_0} :catch_1
    .catch Ljava/lang/IllegalAccessException; {:try_start_0 .. :try_end_0} :catch_2

    .line 409
    :cond_1
    return-object v0

    .line 410
    :catch_0
    move-exception v0

    .line 411
    new-instance v1, La/a/a/a/KC_e$KC_a;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Unable to instantiate fragment "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, ": make sure class name exists, is public, and has an empty constructor that is public"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v1, v2, v0}, La/a/a/a/KC_e$KC_a;-><init>(Ljava/lang/String;Ljava/lang/Exception;)V

    throw v1

    .line 414
    :catch_1
    move-exception v0

    .line 415
    new-instance v1, La/a/a/a/KC_e$KC_a;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Unable to instantiate fragment "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, ": make sure class name exists, is public, and has an empty constructor that is public"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v1, v2, v0}, La/a/a/a/KC_e$KC_a;-><init>(Ljava/lang/String;Ljava/lang/Exception;)V

    throw v1

    .line 418
    :catch_2
    move-exception v0

    .line 419
    new-instance v1, La/a/a/a/KC_e$KC_a;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Unable to instantiate fragment "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, ": make sure class name exists, is public, and has an empty constructor that is public"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-direct {v1, v2, v0}, La/a/a/a/KC_e$KC_a;-><init>(Ljava/lang/String;Ljava/lang/Exception;)V

    throw v1
.end method

.method static b(Landroid/content/Context;Ljava/lang/String;)Z
    .locals 2

    .prologue
    .line 435
    :try_start_0
    sget-object v0, La/a/a/a/KC_e;->M:La/a/a/d/KC_e;

    invoke-virtual {v0, p1}, La/a/a/d/KC_e;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/lang/Class;

    .line 436
    if-nez v0, :cond_0

    .line 438
    invoke-virtual {p0}, Landroid/content/Context;->getClassLoader()Ljava/lang/ClassLoader;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/ClassLoader;->loadClass(Ljava/lang/String;)Ljava/lang/Class;

    move-result-object v0

    .line 439
    sget-object v1, La/a/a/a/KC_e;->M:La/a/a/d/KC_e;

    invoke-virtual {v1, p1, v0}, La/a/a/d/KC_e;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    .line 441
    :cond_0
    const-class v1, La/a/a/a/KC_e;

    invoke-virtual {v1, v0}, Ljava/lang/Class;->isAssignableFrom(Ljava/lang/Class;)Z
    :try_end_0
    .catch Ljava/lang/ClassNotFoundException; {:try_start_0 .. :try_end_0} :catch_0

    move-result v0

    .line 443
    :goto_0
    return v0

    :catch_0
    move-exception v0

    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static j()V
    .locals 0

    .prologue
    .line 766
    return-void
.end method

.method public static l()Landroid/view/animation/Animation;
    .locals 1

    .prologue
    .line 978
    const/4 v0, 0x0

    return-object v0
.end method

.method public static m()V
    .locals 0

    .prologue
    .line 1034
    return-void
.end method

.method public static s()V
    .locals 0

    .prologue
    .line 1266
    return-void
.end method


# virtual methods
.method final A()V
    .locals 3

    .prologue
    .line 1720
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1721
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->p()V

    .line 1723
    :cond_0
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1724
    invoke-virtual {p0}, La/a/a/a/KC_e;->q()V

    .line 1725
    iget-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    if-nez v0, :cond_1

    .line 1726
    new-instance v0, La/a/a/a/KC_q;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " did not call through to super.onDestroy()"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, La/a/a/a/KC_q;-><init>(Ljava/lang/String;)V

    throw v0

    .line 1729
    :cond_1
    return-void
.end method

.method public a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 1

    .prologue
    .line 1020
    const/4 v0, 0x0

    return-object v0
.end method

.method public a()V
    .locals 1

    .prologue
    .line 1222
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1223
    return-void
.end method

.method public a(IILandroid/content/Intent;)V
    .locals 0

    .prologue
    .line 909
    return-void
.end method

.method final a(ILa/a/a/a/KC_e;)V
    .locals 2

    .prologue
    .line 461
    iput p1, p0, La/a/a/a/KC_e;->f:I

    .line 462
    if-eqz p2, :cond_0

    .line 463
    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v1, p2, La/a/a/a/KC_e;->g:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, ":"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    iget v1, p0, La/a/a/a/KC_e;->f:I

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_e;->g:Ljava/lang/String;

    .line 467
    :goto_0
    return-void

    .line 465
    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "android:fragment:"

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget v1, p0, La/a/a/a/KC_e;->f:I

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_e;->g:Ljava/lang/String;

    goto :goto_0
.end method

.method public a(Landroid/app/Activity;)V
    .locals 1

    .prologue
    .line 971
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 972
    return-void
.end method

.method public final a(Landroid/content/Intent;)V
    .locals 3

    .prologue
    .line 877
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    if-nez v0, :cond_0

    .line 878
    new-instance v0, Ljava/lang/IllegalStateException;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " not attached to Activity"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 880
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    const/4 v1, -0x1

    invoke-virtual {v0, p0, p1, v1}, La/a/a/a/KC_f;->startActivityFromFragment(La/a/a/a/KC_e;Landroid/content/Intent;I)V

    .line 881
    return-void
.end method

.method public final a(Landroid/content/Intent;I)V
    .locals 3

    .prologue
    .line 888
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    if-nez v0, :cond_0

    .line 889
    new-instance v0, Ljava/lang/IllegalStateException;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " not attached to Activity"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 891
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    const/16 v1, 0x3039

    invoke-virtual {v0, p0, p1, v1}, La/a/a/a/KC_f;->startActivityFromFragment(La/a/a/a/KC_e;Landroid/content/Intent;I)V

    .line 892
    return-void
.end method

.method final a(Landroid/content/res/Configuration;)V
    .locals 1

    .prologue
    .line 1559
    invoke-virtual {p0, p1}, La/a/a/a/KC_e;->onConfigurationChanged(Landroid/content/res/Configuration;)V

    .line 1560
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1561
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0, p1}, La/a/a/a/KC_i;->a(Landroid/content/res/Configuration;)V

    .line 1563
    :cond_0
    return-void
.end method

.method public a(Landroid/os/Bundle;)V
    .locals 1

    .prologue
    .line 996
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 997
    return-void
.end method

.method public final a(Ljava/lang/String;Ljava/io/FileDescriptor;Ljava/io/PrintWriter;[Ljava/lang/String;)V
    .locals 3

    .prologue
    .line 1377
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mFragmentId=#"

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1378
    iget v0, p0, La/a/a/a/KC_e;->w:I

    invoke-static {v0}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1379
    const-string v0, " mContainerId=#"

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1380
    iget v0, p0, La/a/a/a/KC_e;->x:I

    invoke-static {v0}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1381
    const-string v0, " mTag="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v0, p0, La/a/a/a/KC_e;->y:Ljava/lang/String;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 1382
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mState="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget v0, p0, La/a/a/a/KC_e;->a:I

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(I)V

    .line 1383
    const-string v0, " mIndex="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget v0, p0, La/a/a/a/KC_e;->f:I

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(I)V

    .line 1384
    const-string v0, " mWho="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v0, p0, La/a/a/a/KC_e;->g:Ljava/lang/String;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1385
    const-string v0, " mBackStackNesting="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget v0, p0, La/a/a/a/KC_e;->r:I

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(I)V

    .line 1386
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mAdded="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_e;->l:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 1387
    const-string v0, " mRemoving="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_e;->m:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 1388
    const-string v0, " mResumed="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_e;->n:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 1389
    const-string v0, " mFromLayout="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_e;->o:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 1390
    const-string v0, " mInLayout="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_e;->p:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Z)V

    .line 1391
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mHidden="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_e;->z:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 1392
    const-string v0, " mDetached="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_e;->A:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 1393
    const-string v0, " mMenuVisible="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_e;->D:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 1394
    const-string v0, " mHasMenu="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const/4 v0, 0x0

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Z)V

    .line 1395
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mRetainInstance="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_e;->B:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 1396
    const-string v0, " mRetaining="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_e;->C:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Z)V

    .line 1397
    const-string v0, " mUserVisibleHint="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-boolean v0, p0, La/a/a/a/KC_e;->K:Z

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Z)V

    .line 1398
    iget-object v0, p0, La/a/a/a/KC_e;->s:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1399
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mFragmentManager="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1400
    iget-object v0, p0, La/a/a/a/KC_e;->s:La/a/a/a/KC_i;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 1402
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    if-eqz v0, :cond_1

    .line 1403
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mActivity="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1404
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 1406
    :cond_1
    iget-object v0, p0, La/a/a/a/KC_e;->v:La/a/a/a/KC_e;

    if-eqz v0, :cond_2

    .line 1407
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mParentFragment="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1408
    iget-object v0, p0, La/a/a/a/KC_e;->v:La/a/a/a/KC_e;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 1410
    :cond_2
    iget-object v0, p0, La/a/a/a/KC_e;->h:Landroid/os/Bundle;

    if-eqz v0, :cond_3

    .line 1411
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mArguments="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v0, p0, La/a/a/a/KC_e;->h:Landroid/os/Bundle;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 1413
    :cond_3
    iget-object v0, p0, La/a/a/a/KC_e;->d:Landroid/os/Bundle;

    if-eqz v0, :cond_4

    .line 1414
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mSavedFragmentState="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1415
    iget-object v0, p0, La/a/a/a/KC_e;->d:Landroid/os/Bundle;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 1417
    :cond_4
    iget-object v0, p0, La/a/a/a/KC_e;->e:Landroid/util/SparseArray;

    if-eqz v0, :cond_5

    .line 1418
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mSavedViewState="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1419
    iget-object v0, p0, La/a/a/a/KC_e;->e:Landroid/util/SparseArray;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 1421
    :cond_5
    iget-object v0, p0, La/a/a/a/KC_e;->i:La/a/a/a/KC_e;

    if-eqz v0, :cond_6

    .line 1422
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mTarget="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v0, p0, La/a/a/a/KC_e;->i:La/a/a/a/KC_e;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/Object;)V

    .line 1423
    const-string v0, " mTargetRequestCode="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1424
    iget v0, p0, La/a/a/a/KC_e;->k:I

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(I)V

    .line 1426
    :cond_6
    iget v0, p0, La/a/a/a/KC_e;->F:I

    if-eqz v0, :cond_7

    .line 1427
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mNextAnim="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget v0, p0, La/a/a/a/KC_e;->F:I

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(I)V

    .line 1429
    :cond_7
    iget-object v0, p0, La/a/a/a/KC_e;->G:Landroid/view/ViewGroup;

    if-eqz v0, :cond_8

    .line 1430
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mContainer="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v0, p0, La/a/a/a/KC_e;->G:Landroid/view/ViewGroup;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 1432
    :cond_8
    iget-object v0, p0, La/a/a/a/KC_e;->H:Landroid/view/View;

    if-eqz v0, :cond_9

    .line 1433
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mView="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v0, p0, La/a/a/a/KC_e;->H:Landroid/view/View;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 1435
    :cond_9
    iget-object v0, p0, La/a/a/a/KC_e;->I:Landroid/view/View;

    if-eqz v0, :cond_a

    .line 1436
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mInnerView="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v0, p0, La/a/a/a/KC_e;->H:Landroid/view/View;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 1438
    :cond_a
    iget-object v0, p0, La/a/a/a/KC_e;->b:Landroid/view/View;

    if-eqz v0, :cond_b

    .line 1439
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mAnimatingAway="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v0, p0, La/a/a/a/KC_e;->b:Landroid/view/View;

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/Object;)V

    .line 1440
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "mStateAfterAnimating="

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    .line 1441
    iget v0, p0, La/a/a/a/KC_e;->c:I

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(I)V

    .line 1443
    :cond_b
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    if-eqz v0, :cond_c

    .line 1444
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "Loader Manager:"

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 1445
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "  "

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1, p2, p3, p4}, La/a/a/a/KC_o;->a(Ljava/lang/String;Ljava/io/FileDescriptor;Ljava/io/PrintWriter;[Ljava/lang/String;)V

    .line 1447
    :cond_c
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_d

    .line 1448
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Child "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v1, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, ":"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 1449
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "  "

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1, p2, p3, p4}, La/a/a/a/KC_i;->a(Ljava/lang/String;Ljava/io/FileDescriptor;Ljava/io/PrintWriter;[Ljava/lang/String;)V

    .line 1451
    :cond_d
    return-void
.end method

.method public final a(Z)V
    .locals 1

    .prologue
    .line 820
    iget-boolean v0, p0, La/a/a/a/KC_e;->D:Z

    if-eq v0, p1, :cond_0

    .line 821
    iput-boolean p1, p0, La/a/a/a/KC_e;->D:Z

    .line 822
    :cond_0
    return-void
.end method

.method final a(Landroid/view/Menu;)Z
    .locals 2

    .prologue
    .line 1596
    const/4 v0, 0x0

    .line 1597
    iget-boolean v1, p0, La/a/a/a/KC_e;->z:Z

    if-nez v1, :cond_0

    .line 1598
    iget-object v1, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v1, :cond_0

    .line 1603
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0, p1}, La/a/a/a/KC_i;->a(Landroid/view/Menu;)Z

    move-result v0

    or-int/lit8 v0, v0, 0x0

    .line 1606
    :cond_0
    return v0
.end method

.method final a(Landroid/view/Menu;Landroid/view/MenuInflater;)Z
    .locals 2

    .prologue
    .line 1582
    const/4 v0, 0x0

    .line 1583
    iget-boolean v1, p0, La/a/a/a/KC_e;->z:Z

    if-nez v1, :cond_0

    .line 1584
    iget-object v1, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v1, :cond_0

    .line 1589
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0, p1, p2}, La/a/a/a/KC_i;->a(Landroid/view/Menu;Landroid/view/MenuInflater;)Z

    move-result v0

    or-int/lit8 v0, v0, 0x0

    .line 1592
    :cond_0
    return v0
.end method

.method final a(Landroid/view/MenuItem;)Z
    .locals 1

    .prologue
    .line 1610
    iget-boolean v0, p0, La/a/a/a/KC_e;->z:Z

    if-nez v0, :cond_0

    .line 1611
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1617
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0, p1}, La/a/a/a/KC_i;->a(Landroid/view/MenuItem;)Z

    move-result v0

    if-eqz v0, :cond_0

    .line 1618
    const/4 v0, 0x1

    .line 1622
    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public b(Landroid/os/Bundle;)Landroid/view/LayoutInflater;
    .locals 1

    .prologue
    .line 917
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    invoke-virtual {v0}, La/a/a/a/KC_f;->getLayoutInflater()Landroid/view/LayoutInflater;

    move-result-object v0

    return-object v0
.end method

.method final b(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;
    .locals 1

    .prologue
    .line 1501
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1502
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->g()V

    .line 1504
    :cond_0
    invoke-virtual {p0, p1, p2, p3}, La/a/a/a/KC_e;->a(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;

    move-result-object v0

    return-object v0
.end method

.method final b(Landroid/view/Menu;)V
    .locals 1

    .prologue
    .line 1640
    iget-boolean v0, p0, La/a/a/a/KC_e;->z:Z

    if-nez v0, :cond_0

    .line 1641
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1645
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0, p1}, La/a/a/a/KC_i;->b(Landroid/view/Menu;)V

    .line 1648
    :cond_0
    return-void
.end method

.method public final b(Z)V
    .locals 2

    .prologue
    .line 842
    iget-boolean v0, p0, La/a/a/a/KC_e;->K:Z

    if-nez v0, :cond_0

    if-eqz p1, :cond_0

    iget v0, p0, La/a/a/a/KC_e;->a:I

    const/4 v1, 0x4

    if-ge v0, v1, :cond_0

    .line 843
    iget-object v0, p0, La/a/a/a/KC_e;->s:La/a/a/a/KC_i;

    invoke-virtual {v0, p0}, La/a/a/a/KC_i;->a(La/a/a/a/KC_e;)V

    .line 845
    :cond_0
    iput-boolean p1, p0, La/a/a/a/KC_e;->K:Z

    .line 846
    if-nez p1, :cond_1

    const/4 v0, 0x1

    :goto_0
    iput-boolean v0, p0, La/a/a/a/KC_e;->J:Z

    .line 847
    return-void

    .line 846
    :cond_1
    const/4 v0, 0x0

    goto :goto_0
.end method

.method final b(Landroid/view/MenuItem;)Z
    .locals 1

    .prologue
    .line 1626
    iget-boolean v0, p0, La/a/a/a/KC_e;->z:Z

    if-nez v0, :cond_0

    .line 1627
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1631
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0, p1}, La/a/a/a/KC_i;->b(Landroid/view/MenuItem;)Z

    move-result v0

    if-eqz v0, :cond_0

    .line 1632
    const/4 v0, 0x1

    .line 1636
    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public c()V
    .locals 4

    .prologue
    const/4 v1, 0x1

    .line 1085
    iput-boolean v1, p0, La/a/a/a/KC_e;->E:Z

    .line 1087
    iget-boolean v0, p0, La/a/a/a/KC_e;->N:Z

    if-nez v0, :cond_1

    .line 1088
    iput-boolean v1, p0, La/a/a/a/KC_e;->N:Z

    .line 1089
    iget-boolean v0, p0, La/a/a/a/KC_e;->O:Z

    if-nez v0, :cond_0

    .line 1090
    iput-boolean v1, p0, La/a/a/a/KC_e;->O:Z

    .line 1091
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    iget-object v1, p0, La/a/a/a/KC_e;->g:Ljava/lang/String;

    iget-boolean v2, p0, La/a/a/a/KC_e;->N:Z

    const/4 v3, 0x0

    invoke-virtual {v0, v1, v2, v3}, La/a/a/a/KC_f;->a(Ljava/lang/String;ZZ)La/a/a/a/KC_o;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    .line 1093
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    if-eqz v0, :cond_1

    .line 1094
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    invoke-virtual {v0}, La/a/a/a/KC_o;->b()V

    .line 1097
    :cond_1
    return-void
.end method

.method public c(Landroid/os/Bundle;)V
    .locals 1

    .prologue
    .line 1061
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1062
    return-void
.end method

.method public d()V
    .locals 1

    .prologue
    .line 1150
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1151
    return-void
.end method

.method public d(Landroid/os/Bundle;)V
    .locals 0

    .prologue
    .line 1129
    return-void
.end method

.method public e()V
    .locals 1

    .prologue
    .line 1167
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1168
    return-void
.end method

.method final e(Landroid/os/Bundle;)V
    .locals 3

    .prologue
    .line 448
    iget-object v0, p0, La/a/a/a/KC_e;->e:Landroid/util/SparseArray;

    if-eqz v0, :cond_0

    .line 449
    iget-object v0, p0, La/a/a/a/KC_e;->I:Landroid/view/View;

    iget-object v1, p0, La/a/a/a/KC_e;->e:Landroid/util/SparseArray;

    invoke-virtual {v0, v1}, Landroid/view/View;->restoreHierarchyState(Landroid/util/SparseArray;)V

    .line 450
    const/4 v0, 0x0

    iput-object v0, p0, La/a/a/a/KC_e;->e:Landroid/util/SparseArray;

    .line 452
    :cond_0
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 453
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 454
    iget-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    if-nez v0, :cond_1

    .line 455
    new-instance v0, La/a/a/a/KC_q;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " did not call through to super.onViewStateRestored()"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, La/a/a/a/KC_q;-><init>(Ljava/lang/String;)V

    throw v0

    .line 458
    :cond_1
    return-void
.end method

.method public final equals(Ljava/lang/Object;)Z
    .locals 1
    .param p1, "o"    # Ljava/lang/Object;

    .prologue
    .line 477
    invoke-super {p0, p1}, Ljava/lang/Object;->equals(Ljava/lang/Object;)Z

    move-result v0

    return v0
.end method

.method public final f(Landroid/os/Bundle;)V
    .locals 2

    .prologue
    .line 531
    iget v0, p0, La/a/a/a/KC_e;->f:I

    if-ltz v0, :cond_0

    .line 532
    new-instance v0, Ljava/lang/IllegalStateException;

    const-string v1, "Fragment already active"

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 534
    :cond_0
    iput-object p1, p0, La/a/a/a/KC_e;->h:Landroid/os/Bundle;

    .line 535
    return-void
.end method

.method final f()Z
    .locals 1

    .prologue
    .line 470
    iget v0, p0, La/a/a/a/KC_e;->r:I

    if-lez v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public final g()Landroid/os/Bundle;
    .locals 1

    .prologue
    .line 542
    iget-object v0, p0, La/a/a/a/KC_e;->h:Landroid/os/Bundle;

    return-object v0
.end method

.method final g(Landroid/os/Bundle;)V
    .locals 4

    .prologue
    .line 1477
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1478
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->g()V

    .line 1480
    :cond_0
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1481
    invoke-virtual {p0, p1}, La/a/a/a/KC_e;->a(Landroid/os/Bundle;)V

    .line 1482
    iget-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    if-nez v0, :cond_1

    .line 1483
    new-instance v0, La/a/a/a/KC_q;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " did not call through to super.onCreate()"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, La/a/a/a/KC_q;-><init>(Ljava/lang/String;)V

    throw v0

    .line 1486
    :cond_1
    if-eqz p1, :cond_3

    .line 1487
    const-string v0, "android:support:fragments"

    invoke-virtual {p1, v0}, Landroid/os/Bundle;->getParcelable(Ljava/lang/String;)Landroid/os/Parcelable;

    move-result-object v0

    .line 1489
    if-eqz v0, :cond_3

    .line 1490
    iget-object v1, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-nez v1, :cond_2

    .line 1491
    new-instance v1, La/a/a/a/KC_i;

    invoke-direct {v1}, La/a/a/a/KC_i;-><init>()V

    iput-object v1, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    iget-object v1, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    iget-object v2, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    new-instance v3, La/a/a/a/KC_e$1;

    invoke-direct {v3, p0}, La/a/a/a/KC_e$1;-><init>(La/a/a/a/KC_e;)V

    invoke-virtual {v1, v2, v3, p0}, La/a/a/a/KC_i;->a(La/a/a/a/KC_f;La/a/a/a/KC_g;La/a/a/a/KC_e;)V

    .line 1493
    :cond_2
    iget-object v1, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    const/4 v2, 0x0

    invoke-virtual {v1, v0, v2}, La/a/a/a/KC_i;->a(Landroid/os/Parcelable;Ljava/util/ArrayList;)V

    .line 1494
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->h()V

    .line 1497
    :cond_3
    return-void
.end method

.method public final h()La/a/a/a/KC_f;
    .locals 1

    .prologue
    .line 595
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    return-object v0
.end method

.method final h(Landroid/os/Bundle;)V
    .locals 3

    .prologue
    .line 1508
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1509
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->g()V

    .line 1511
    :cond_0
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1512
    invoke-virtual {p0, p1}, La/a/a/a/KC_e;->c(Landroid/os/Bundle;)V

    .line 1513
    iget-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    if-nez v0, :cond_1

    .line 1514
    new-instance v0, La/a/a/a/KC_q;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " did not call through to super.onActivityCreated()"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, La/a/a/a/KC_q;-><init>(Ljava/lang/String;)V

    throw v0

    .line 1517
    :cond_1
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_2

    .line 1518
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->i()V

    .line 1520
    :cond_2
    return-void
.end method

.method public final hashCode()I
    .locals 1

    .prologue
    .line 484
    invoke-super {p0}, Ljava/lang/Object;->hashCode()I

    move-result v0

    return v0
.end method

.method public final i()Landroid/content/res/Resources;
    .locals 3

    .prologue
    .line 602
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    if-nez v0, :cond_0

    .line 603
    new-instance v0, Ljava/lang/IllegalStateException;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " not attached to Activity"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, Ljava/lang/IllegalStateException;-><init>(Ljava/lang/String;)V

    throw v0

    .line 605
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    invoke-virtual {v0}, La/a/a/a/KC_f;->getResources()Landroid/content/res/Resources;

    move-result-object v0

    return-object v0
.end method

.method final i(Landroid/os/Bundle;)V
    .locals 2

    .prologue
    .line 1651
    invoke-virtual {p0, p1}, La/a/a/a/KC_e;->d(Landroid/os/Bundle;)V

    .line 1652
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1653
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->f()Landroid/os/Parcelable;

    move-result-object v0

    .line 1654
    if-eqz v0, :cond_0

    .line 1655
    const-string v1, "android:support:fragments"

    invoke-virtual {p1, v1, v0}, Landroid/os/Bundle;->putParcelable(Ljava/lang/String;Landroid/os/Parcelable;)V

    .line 1658
    :cond_0
    return-void
.end method

.method public final k()V
    .locals 1

    .prologue
    .line 963
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 964
    return-void
.end method

.method public final n()Landroid/view/View;
    .locals 1

    .prologue
    .line 1044
    iget-object v0, p0, La/a/a/a/KC_e;->H:Landroid/view/View;

    return-object v0
.end method

.method public o()V
    .locals 1

    .prologue
    .line 1106
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1107
    return-void
.end method

.method public onConfigurationChanged(Landroid/content/res/Configuration;)V
    .locals 1
    .param p1, "newConfig"    # Landroid/content/res/Configuration;

    .prologue
    .line 1132
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1133
    return-void
.end method

.method public onCreateContextMenu(Landroid/view/ContextMenu;Landroid/view/View;Landroid/view/ContextMenu$ContextMenuInfo;)V
    .locals 1
    .param p1, "menu"    # Landroid/view/ContextMenu;
    .param p2, "v"    # Landroid/view/View;
    .param p3, "menuInfo"    # Landroid/view/ContextMenu$ContextMenuInfo;

    .prologue
    .line 1318
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    invoke-virtual {v0, p1, p2, p3}, La/a/a/a/KC_f;->onCreateContextMenu(Landroid/view/ContextMenu;Landroid/view/View;Landroid/view/ContextMenu$ContextMenuInfo;)V

    .line 1319
    return-void
.end method

.method public onLowMemory()V
    .locals 1

    .prologue
    .line 1154
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1155
    return-void
.end method

.method public p()V
    .locals 1

    .prologue
    .line 1141
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1142
    return-void
.end method

.method public q()V
    .locals 4

    .prologue
    const/4 v1, 0x1

    .line 1175
    iput-boolean v1, p0, La/a/a/a/KC_e;->E:Z

    .line 1178
    iget-boolean v0, p0, La/a/a/a/KC_e;->O:Z

    if-nez v0, :cond_0

    .line 1179
    iput-boolean v1, p0, La/a/a/a/KC_e;->O:Z

    .line 1180
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    iget-object v1, p0, La/a/a/a/KC_e;->g:Ljava/lang/String;

    iget-boolean v2, p0, La/a/a/a/KC_e;->N:Z

    const/4 v3, 0x0

    invoke-virtual {v0, v1, v2, v3}, La/a/a/a/KC_f;->a(Ljava/lang/String;ZZ)La/a/a/a/KC_o;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    .line 1182
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    if-eqz v0, :cond_1

    .line 1183
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    invoke-virtual {v0}, La/a/a/a/KC_o;->h()V

    .line 1185
    :cond_1
    return-void
.end method

.method final r()V
    .locals 3

    .prologue
    const/4 v2, 0x0

    const/4 v1, 0x0

    .line 1194
    const/4 v0, -0x1

    iput v0, p0, La/a/a/a/KC_e;->f:I

    .line 1195
    iput-object v2, p0, La/a/a/a/KC_e;->g:Ljava/lang/String;

    .line 1196
    iput-boolean v1, p0, La/a/a/a/KC_e;->l:Z

    .line 1197
    iput-boolean v1, p0, La/a/a/a/KC_e;->m:Z

    .line 1198
    iput-boolean v1, p0, La/a/a/a/KC_e;->n:Z

    .line 1199
    iput-boolean v1, p0, La/a/a/a/KC_e;->o:Z

    .line 1200
    iput-boolean v1, p0, La/a/a/a/KC_e;->p:Z

    .line 1201
    iput-boolean v1, p0, La/a/a/a/KC_e;->q:Z

    .line 1202
    iput v1, p0, La/a/a/a/KC_e;->r:I

    .line 1203
    iput-object v2, p0, La/a/a/a/KC_e;->s:La/a/a/a/KC_i;

    .line 1204
    iput-object v2, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    .line 1205
    iput-object v2, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    .line 1206
    iput v1, p0, La/a/a/a/KC_e;->w:I

    .line 1207
    iput v1, p0, La/a/a/a/KC_e;->x:I

    .line 1208
    iput-object v2, p0, La/a/a/a/KC_e;->y:Ljava/lang/String;

    .line 1209
    iput-boolean v1, p0, La/a/a/a/KC_e;->z:Z

    .line 1210
    iput-boolean v1, p0, La/a/a/a/KC_e;->A:Z

    .line 1211
    iput-boolean v1, p0, La/a/a/a/KC_e;->C:Z

    .line 1212
    iput-object v2, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    .line 1213
    iput-boolean v1, p0, La/a/a/a/KC_e;->N:Z

    .line 1214
    iput-boolean v1, p0, La/a/a/a/KC_e;->O:Z

    .line 1215
    return-void
.end method

.method final t()V
    .locals 3

    .prologue
    .line 1523
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1524
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->g()V

    .line 1525
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->d()Z

    .line 1527
    :cond_0
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1528
    invoke-virtual {p0}, La/a/a/a/KC_e;->c()V

    .line 1529
    iget-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    if-nez v0, :cond_1

    .line 1530
    new-instance v0, La/a/a/a/KC_q;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " did not call through to super.onStart()"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, La/a/a/a/KC_q;-><init>(Ljava/lang/String;)V

    throw v0

    .line 1533
    :cond_1
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_2

    .line 1534
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->j()V

    .line 1536
    :cond_2
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    if-eqz v0, :cond_3

    .line 1537
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    invoke-virtual {v0}, La/a/a/a/KC_o;->g()V

    .line 1539
    :cond_3
    return-void
.end method

.method public toString()Ljava/lang/String;
    .locals 2

    .prologue
    .line 489
    new-instance v0, Ljava/lang/StringBuilder;

    const/16 v1, 0x80

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(I)V

    .line 490
    invoke-static {p0, v0}, La/a/a/c/KC_a;->a(Ljava/lang/Object;Ljava/lang/StringBuilder;)V

    .line 491
    iget v1, p0, La/a/a/a/KC_e;->f:I

    if-ltz v1, :cond_0

    .line 492
    const-string v1, " #"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 493
    iget v1, p0, La/a/a/a/KC_e;->f:I

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    .line 495
    :cond_0
    iget v1, p0, La/a/a/a/KC_e;->w:I

    if-eqz v1, :cond_1

    .line 496
    const-string v1, " id=0x"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 497
    iget v1, p0, La/a/a/a/KC_e;->w:I

    invoke-static {v1}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 499
    :cond_1
    iget-object v1, p0, La/a/a/a/KC_e;->y:Ljava/lang/String;

    if-eqz v1, :cond_2

    .line 500
    const-string v1, " "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 501
    iget-object v1, p0, La/a/a/a/KC_e;->y:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 503
    :cond_2
    const/16 v1, 0x7d

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(C)Ljava/lang/StringBuilder;

    .line 504
    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method final u()V
    .locals 3

    .prologue
    .line 1542
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1543
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->g()V

    .line 1544
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->d()Z

    .line 1546
    :cond_0
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1547
    invoke-virtual {p0}, La/a/a/a/KC_e;->o()V

    .line 1548
    iget-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    if-nez v0, :cond_1

    .line 1549
    new-instance v0, La/a/a/a/KC_q;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " did not call through to super.onResume()"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, La/a/a/a/KC_q;-><init>(Ljava/lang/String;)V

    throw v0

    .line 1552
    :cond_1
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_2

    .line 1553
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->k()V

    .line 1554
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->d()Z

    .line 1556
    :cond_2
    return-void
.end method

.method final v()V
    .locals 1

    .prologue
    .line 1566
    invoke-virtual {p0}, La/a/a/a/KC_e;->onLowMemory()V

    .line 1567
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1568
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->q()V

    .line 1570
    :cond_0
    return-void
.end method

.method final w()V
    .locals 3

    .prologue
    .line 1661
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1662
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->l()V

    .line 1664
    :cond_0
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1665
    invoke-virtual {p0}, La/a/a/a/KC_e;->p()V

    .line 1666
    iget-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    if-nez v0, :cond_1

    .line 1667
    new-instance v0, La/a/a/a/KC_q;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " did not call through to super.onPause()"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, La/a/a/a/KC_q;-><init>(Ljava/lang/String;)V

    throw v0

    .line 1670
    :cond_1
    return-void
.end method

.method final x()V
    .locals 3

    .prologue
    .line 1673
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1674
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->m()V

    .line 1676
    :cond_0
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1677
    invoke-virtual {p0}, La/a/a/a/KC_e;->d()V

    .line 1678
    iget-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    if-nez v0, :cond_1

    .line 1679
    new-instance v0, La/a/a/a/KC_q;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " did not call through to super.onStop()"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, La/a/a/a/KC_q;-><init>(Ljava/lang/String;)V

    throw v0

    .line 1682
    :cond_1
    return-void
.end method

.method final y()V
    .locals 4

    .prologue
    const/4 v3, 0x0

    .line 1685
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1686
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->n()V

    .line 1688
    :cond_0
    iget-boolean v0, p0, La/a/a/a/KC_e;->N:Z

    if-eqz v0, :cond_2

    .line 1689
    iput-boolean v3, p0, La/a/a/a/KC_e;->N:Z

    .line 1690
    iget-boolean v0, p0, La/a/a/a/KC_e;->O:Z

    if-nez v0, :cond_1

    .line 1691
    const/4 v0, 0x1

    iput-boolean v0, p0, La/a/a/a/KC_e;->O:Z

    .line 1692
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    iget-object v1, p0, La/a/a/a/KC_e;->g:Ljava/lang/String;

    iget-boolean v2, p0, La/a/a/a/KC_e;->N:Z

    invoke-virtual {v0, v1, v2, v3}, La/a/a/a/KC_f;->a(Ljava/lang/String;ZZ)La/a/a/a/KC_o;

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    .line 1694
    :cond_1
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    if-eqz v0, :cond_2

    .line 1695
    iget-object v0, p0, La/a/a/a/KC_e;->t:La/a/a/a/KC_f;

    iget-boolean v0, v0, La/a/a/a/KC_f;->d:Z

    if-nez v0, :cond_3

    .line 1696
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    invoke-virtual {v0}, La/a/a/a/KC_o;->c()V

    .line 1702
    :cond_2
    :goto_0
    return-void

    .line 1698
    :cond_3
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    invoke-virtual {v0}, La/a/a/a/KC_o;->d()V

    goto :goto_0
.end method

.method final z()V
    .locals 3

    .prologue
    .line 1705
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    if-eqz v0, :cond_0

    .line 1706
    iget-object v0, p0, La/a/a/a/KC_e;->u:La/a/a/a/KC_i;

    invoke-virtual {v0}, La/a/a/a/KC_i;->o()V

    .line 1708
    :cond_0
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    .line 1709
    invoke-virtual {p0}, La/a/a/a/KC_e;->e()V

    .line 1710
    iget-boolean v0, p0, La/a/a/a/KC_e;->E:Z

    if-nez v0, :cond_1

    .line 1711
    new-instance v0, La/a/a/a/KC_q;

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Fragment "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " did not call through to super.onDestroyView()"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-direct {v0, v1}, La/a/a/a/KC_q;-><init>(Ljava/lang/String;)V

    throw v0

    .line 1714
    :cond_1
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    if-eqz v0, :cond_2

    .line 1715
    iget-object v0, p0, La/a/a/a/KC_e;->L:La/a/a/a/KC_o;

    invoke-virtual {v0}, La/a/a/a/KC_o;->f()V

    .line 1717
    :cond_2
    return-void
.end method
