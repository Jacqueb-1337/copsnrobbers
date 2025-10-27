.class final La/a/a/a/KC_o;
.super La/a/a/a/KC_n;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/a/KC_o$KC_a;
    }
.end annotation


# static fields
.field static a:Z


# instance fields
.field final b:Ljava/lang/String;

.field c:La/a/a/a/KC_f;

.field d:Z

.field e:Z

.field private f:La/a/a/d/KC_f;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "La/a/a/d/KC_f",
            "<",
            "La/a/a/a/KC_o$KC_a;",
            ">;"
        }
    .end annotation
.end field

.field private g:La/a/a/d/KC_f;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "La/a/a/d/KC_f",
            "<",
            "La/a/a/a/KC_o$KC_a;",
            ">;"
        }
    .end annotation
.end field


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 189
    const/4 v0, 0x0

    sput-boolean v0, La/a/a/a/KC_o;->a:Z

    return-void
.end method

.method constructor <init>(Ljava/lang/String;La/a/a/a/KC_f;Z)V
    .locals 1

    .prologue
    .line 477
    invoke-direct {p0}, La/a/a/a/KC_n;-><init>()V

    .line 194
    new-instance v0, La/a/a/d/KC_f;

    invoke-direct {v0}, La/a/a/d/KC_f;-><init>()V

    iput-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    .line 200
    new-instance v0, La/a/a/d/KC_f;

    invoke-direct {v0}, La/a/a/d/KC_f;-><init>()V

    iput-object v0, p0, La/a/a/a/KC_o;->g:La/a/a/d/KC_f;

    .line 478
    iput-object p1, p0, La/a/a/a/KC_o;->b:Ljava/lang/String;

    .line 479
    iput-object p2, p0, La/a/a/a/KC_o;->c:La/a/a/a/KC_f;

    .line 480
    iput-boolean p3, p0, La/a/a/a/KC_o;->d:Z

    .line 481
    return-void
.end method


# virtual methods
.method final a(La/a/a/a/KC_f;)V
    .locals 0

    .prologue
    .line 484
    iput-object p1, p0, La/a/a/a/KC_o;->c:La/a/a/a/KC_f;

    .line 485
    return-void
.end method

.method public final a(Ljava/lang/String;Ljava/io/FileDescriptor;Ljava/io/PrintWriter;[Ljava/lang/String;)V
    .locals 5

    .prologue
    const/4 v2, 0x0

    .line 801
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    if-lez v0, :cond_0

    .line 802
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "Active Loaders:"

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 803
    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "    "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    move v1, v2

    .line 804
    :goto_0
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    if-ge v1, v0, :cond_0

    .line 805
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0, v1}, La/a/a/d/KC_f;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o$KC_a;

    .line 806
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v4, "  #"

    invoke-virtual {p3, v4}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v4, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v4, v1}, La/a/a/d/KC_f;->a(I)I

    move-result v4

    invoke-virtual {p3, v4}, Ljava/io/PrintWriter;->print(I)V

    .line 807
    const-string v4, ": "

    invoke-virtual {p3, v4}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    invoke-virtual {v0}, La/a/a/a/KC_o$KC_a;->toString()Ljava/lang/String;

    move-result-object v4

    invoke-virtual {p3, v4}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 808
    invoke-virtual {v0, v3, p2, p3, p4}, La/a/a/a/KC_o$KC_a;->a(Ljava/lang/String;Ljava/io/FileDescriptor;Ljava/io/PrintWriter;[Ljava/lang/String;)V

    .line 804
    add-int/lit8 v0, v1, 0x1

    move v1, v0

    goto :goto_0

    .line 811
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_o;->g:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    if-lez v0, :cond_1

    .line 812
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v0, "Inactive Loaders:"

    invoke-virtual {p3, v0}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 813
    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "    "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    .line 814
    :goto_1
    iget-object v0, p0, La/a/a/a/KC_o;->g:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    if-ge v2, v0, :cond_1

    .line 815
    iget-object v0, p0, La/a/a/a/KC_o;->g:La/a/a/d/KC_f;

    invoke-virtual {v0, v2}, La/a/a/d/KC_f;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o$KC_a;

    .line 816
    invoke-virtual {p3, p1}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    const-string v3, "  #"

    invoke-virtual {p3, v3}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    iget-object v3, p0, La/a/a/a/KC_o;->g:La/a/a/d/KC_f;

    invoke-virtual {v3, v2}, La/a/a/d/KC_f;->a(I)I

    move-result v3

    invoke-virtual {p3, v3}, Ljava/io/PrintWriter;->print(I)V

    .line 817
    const-string v3, ": "

    invoke-virtual {p3, v3}, Ljava/io/PrintWriter;->print(Ljava/lang/String;)V

    invoke-virtual {v0}, La/a/a/a/KC_o$KC_a;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {p3, v3}, Ljava/io/PrintWriter;->println(Ljava/lang/String;)V

    .line 818
    invoke-virtual {v0, v1, p2, p3, p4}, La/a/a/a/KC_o$KC_a;->a(Ljava/lang/String;Ljava/io/FileDescriptor;Ljava/io/PrintWriter;[Ljava/lang/String;)V

    .line 814
    add-int/lit8 v2, v2, 0x1

    goto :goto_1

    .line 821
    :cond_1
    return-void
.end method

.method public final a()Z
    .locals 6

    .prologue
    const/4 v1, 0x0

    .line 825
    .line 826
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v4

    move v2, v1

    move v3, v1

    .line 827
    :goto_0
    if-ge v2, v4, :cond_1

    .line 828
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0, v2}, La/a/a/d/KC_f;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o$KC_a;

    .line 829
    iget-boolean v5, v0, La/a/a/a/KC_o$KC_a;->e:Z

    if-eqz v5, :cond_0

    iget-boolean v0, v0, La/a/a/a/KC_o$KC_a;->d:Z

    if-nez v0, :cond_0

    const/4 v0, 0x1

    :goto_1
    or-int/2addr v3, v0

    .line 827
    add-int/lit8 v0, v2, 0x1

    move v2, v0

    goto :goto_0

    :cond_0
    move v0, v1

    .line 829
    goto :goto_1

    .line 831
    :cond_1
    return v3
.end method

.method final b()V
    .locals 5

    .prologue
    const/4 v4, 0x1

    .line 701
    iget-boolean v0, p0, La/a/a/a/KC_o;->d:Z

    if-eqz v0, :cond_1

    .line 703
    new-instance v0, Ljava/lang/RuntimeException;

    const-string v1, "here"

    invoke-direct {v0, v1}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/String;)V

    .line 704
    invoke-virtual {v0}, Ljava/lang/RuntimeException;->fillInStackTrace()Ljava/lang/Throwable;

    .line 705
    const-string v1, "LoaderManager"

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Called doStart when already started: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v1, v2, v0}, Landroid/util/Log;->w(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Throwable;)I

    .line 716
    :cond_0
    return-void

    .line 709
    :cond_1
    iput-boolean v4, p0, La/a/a/a/KC_o;->d:Z

    .line 713
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    add-int/lit8 v0, v0, -0x1

    move v1, v0

    :goto_0
    if-ltz v1, :cond_0

    .line 714
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0, v1}, La/a/a/d/KC_f;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o$KC_a;

    iget-boolean v2, v0, La/a/a/a/KC_o$KC_a;->f:Z

    if-eqz v2, :cond_3

    iget-boolean v2, v0, La/a/a/a/KC_o$KC_a;->g:Z

    if-eqz v2, :cond_3

    iput-boolean v4, v0, La/a/a/a/KC_o$KC_a;->e:Z

    .line 713
    :cond_2
    :goto_1
    add-int/lit8 v0, v1, -0x1

    move v1, v0

    goto :goto_0

    .line 714
    :cond_3
    iget-boolean v2, v0, La/a/a/a/KC_o$KC_a;->e:Z

    if-nez v2, :cond_2

    iput-boolean v4, v0, La/a/a/a/KC_o$KC_a;->e:Z

    iget-object v2, v0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    iget-object v2, v0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    if-eqz v2, :cond_2

    iget-object v2, v0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    invoke-virtual {v2}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/Class;->isMemberClass()Z

    move-result v2

    if-eqz v2, :cond_4

    iget-object v2, v0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    invoke-virtual {v2}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/Class;->getModifiers()I

    move-result v2

    invoke-static {v2}, Ljava/lang/reflect/Modifier;->isStatic(I)Z

    move-result v2

    if-nez v2, :cond_4

    new-instance v1, Ljava/lang/IllegalArgumentException;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Object returned from onCreateLoader must not be a non-static inner member class: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v0, v0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-direct {v1, v0}, Ljava/lang/IllegalArgumentException;-><init>(Ljava/lang/String;)V

    throw v1

    :cond_4
    iget-boolean v2, v0, La/a/a/a/KC_o$KC_a;->i:Z

    if-nez v2, :cond_5

    iget-object v2, v0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    iget v3, v0, La/a/a/a/KC_o$KC_a;->a:I

    invoke-virtual {v2, v3, v0}, La/a/a/b/KC_a;->a(ILa/a/a/b/KC_a$KC_a;)V

    iput-boolean v4, v0, La/a/a/a/KC_o$KC_a;->i:Z

    :cond_5
    iget-object v0, v0, La/a/a/a/KC_o$KC_a;->c:La/a/a/b/KC_a;

    invoke-virtual {v0}, La/a/a/b/KC_a;->a()V

    goto :goto_1
.end method

.method final c()V
    .locals 4

    .prologue
    .line 719
    iget-boolean v0, p0, La/a/a/a/KC_o;->d:Z

    if-nez v0, :cond_0

    .line 721
    new-instance v0, Ljava/lang/RuntimeException;

    const-string v1, "here"

    invoke-direct {v0, v1}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/String;)V

    .line 722
    invoke-virtual {v0}, Ljava/lang/RuntimeException;->fillInStackTrace()Ljava/lang/Throwable;

    .line 723
    const-string v1, "LoaderManager"

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Called doStop when not started: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v1, v2, v0}, Landroid/util/Log;->w(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Throwable;)I

    .line 731
    :goto_0
    return-void

    .line 727
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    add-int/lit8 v0, v0, -0x1

    move v1, v0

    :goto_1
    if-ltz v1, :cond_1

    .line 728
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0, v1}, La/a/a/d/KC_f;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o$KC_a;

    invoke-virtual {v0}, La/a/a/a/KC_o$KC_a;->a()V

    .line 727
    add-int/lit8 v0, v1, -0x1

    move v1, v0

    goto :goto_1

    .line 730
    :cond_1
    const/4 v0, 0x0

    iput-boolean v0, p0, La/a/a/a/KC_o;->d:Z

    goto :goto_0
.end method

.method final d()V
    .locals 5

    .prologue
    const/4 v4, 0x1

    const/4 v3, 0x0

    .line 734
    iget-boolean v0, p0, La/a/a/a/KC_o;->d:Z

    if-nez v0, :cond_1

    .line 736
    new-instance v0, Ljava/lang/RuntimeException;

    const-string v1, "here"

    invoke-direct {v0, v1}, Ljava/lang/RuntimeException;-><init>(Ljava/lang/String;)V

    .line 737
    invoke-virtual {v0}, Ljava/lang/RuntimeException;->fillInStackTrace()Ljava/lang/Throwable;

    .line 738
    const-string v1, "LoaderManager"

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Called doRetain when not started: "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v1, v2, v0}, Landroid/util/Log;->w(Ljava/lang/String;Ljava/lang/String;Ljava/lang/Throwable;)I

    .line 747
    :cond_0
    return-void

    .line 742
    :cond_1
    iput-boolean v4, p0, La/a/a/a/KC_o;->e:Z

    .line 743
    iput-boolean v3, p0, La/a/a/a/KC_o;->d:Z

    .line 744
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    add-int/lit8 v0, v0, -0x1

    move v1, v0

    :goto_0
    if-ltz v1, :cond_0

    .line 745
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0, v1}, La/a/a/d/KC_f;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o$KC_a;

    iput-boolean v4, v0, La/a/a/a/KC_o$KC_a;->f:Z

    iget-boolean v2, v0, La/a/a/a/KC_o$KC_a;->e:Z

    iput-boolean v2, v0, La/a/a/a/KC_o$KC_a;->g:Z

    iput-boolean v3, v0, La/a/a/a/KC_o$KC_a;->e:Z

    const/4 v2, 0x0

    iput-object v2, v0, La/a/a/a/KC_o$KC_a;->b:La/a/a/a/KC_h$KC_a;

    .line 744
    add-int/lit8 v0, v1, -0x1

    move v1, v0

    goto :goto_0
.end method

.method final e()V
    .locals 5

    .prologue
    const/4 v4, 0x0

    .line 750
    iget-boolean v0, p0, La/a/a/a/KC_o;->e:Z

    if-eqz v0, :cond_1

    .line 751
    iput-boolean v4, p0, La/a/a/a/KC_o;->e:Z

    .line 754
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    add-int/lit8 v0, v0, -0x1

    move v1, v0

    :goto_0
    if-ltz v1, :cond_1

    .line 755
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0, v1}, La/a/a/d/KC_f;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o$KC_a;

    iget-boolean v2, v0, La/a/a/a/KC_o$KC_a;->f:Z

    if-eqz v2, :cond_0

    iput-boolean v4, v0, La/a/a/a/KC_o$KC_a;->f:Z

    iget-boolean v2, v0, La/a/a/a/KC_o$KC_a;->e:Z

    iget-boolean v3, v0, La/a/a/a/KC_o$KC_a;->g:Z

    if-eq v2, v3, :cond_0

    iget-boolean v2, v0, La/a/a/a/KC_o$KC_a;->e:Z

    if-nez v2, :cond_0

    invoke-virtual {v0}, La/a/a/a/KC_o$KC_a;->a()V

    :cond_0
    iget-boolean v0, v0, La/a/a/a/KC_o$KC_a;->e:Z

    .line 754
    add-int/lit8 v0, v1, -0x1

    move v1, v0

    goto :goto_0

    .line 758
    :cond_1
    return-void
.end method

.method final f()V
    .locals 3

    .prologue
    .line 761
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    add-int/lit8 v0, v0, -0x1

    move v1, v0

    :goto_0
    if-ltz v1, :cond_0

    .line 762
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0, v1}, La/a/a/d/KC_f;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o$KC_a;

    const/4 v2, 0x1

    iput-boolean v2, v0, La/a/a/a/KC_o$KC_a;->h:Z

    .line 761
    add-int/lit8 v0, v1, -0x1

    move v1, v0

    goto :goto_0

    .line 764
    :cond_0
    return-void
.end method

.method final g()V
    .locals 3

    .prologue
    .line 767
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    add-int/lit8 v0, v0, -0x1

    move v1, v0

    :goto_0
    if-ltz v1, :cond_1

    .line 768
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0, v1}, La/a/a/d/KC_f;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o$KC_a;

    iget-boolean v2, v0, La/a/a/a/KC_o$KC_a;->e:Z

    if-eqz v2, :cond_0

    iget-boolean v2, v0, La/a/a/a/KC_o$KC_a;->h:Z

    if-eqz v2, :cond_0

    const/4 v2, 0x0

    iput-boolean v2, v0, La/a/a/a/KC_o$KC_a;->h:Z

    .line 767
    :cond_0
    add-int/lit8 v0, v1, -0x1

    move v1, v0

    goto :goto_0

    .line 770
    :cond_1
    return-void
.end method

.method final h()V
    .locals 2

    .prologue
    .line 773
    iget-boolean v0, p0, La/a/a/a/KC_o;->e:Z

    if-nez v0, :cond_1

    .line 774
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    add-int/lit8 v0, v0, -0x1

    move v1, v0

    :goto_0
    if-ltz v1, :cond_0

    .line 776
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0, v1}, La/a/a/d/KC_f;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o$KC_a;

    invoke-virtual {v0}, La/a/a/a/KC_o$KC_a;->b()V

    .line 775
    add-int/lit8 v0, v1, -0x1

    move v1, v0

    goto :goto_0

    .line 778
    :cond_0
    iget-object v0, p0, La/a/a/a/KC_o;->f:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->b()V

    .line 781
    :cond_1
    iget-object v0, p0, La/a/a/a/KC_o;->g:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->a()I

    move-result v0

    add-int/lit8 v0, v0, -0x1

    move v1, v0

    :goto_1
    if-ltz v1, :cond_2

    .line 783
    iget-object v0, p0, La/a/a/a/KC_o;->g:La/a/a/d/KC_f;

    invoke-virtual {v0, v1}, La/a/a/d/KC_f;->b(I)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, La/a/a/a/KC_o$KC_a;

    invoke-virtual {v0}, La/a/a/a/KC_o$KC_a;->b()V

    .line 782
    add-int/lit8 v0, v1, -0x1

    move v1, v0

    goto :goto_1

    .line 785
    :cond_2
    iget-object v0, p0, La/a/a/a/KC_o;->g:La/a/a/d/KC_f;

    invoke-virtual {v0}, La/a/a/d/KC_f;->b()V

    .line 786
    return-void
.end method

.method public final toString()Ljava/lang/String;
    .locals 2

    .prologue
    .line 790
    new-instance v0, Ljava/lang/StringBuilder;

    const/16 v1, 0x80

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(I)V

    .line 791
    const-string v1, "LoaderManager{"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 792
    invoke-static {p0}, Ljava/lang/System;->identityHashCode(Ljava/lang/Object;)I

    move-result v1

    invoke-static {v1}, Ljava/lang/Integer;->toHexString(I)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 793
    const-string v1, " in "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 794
    iget-object v1, p0, La/a/a/a/KC_o;->c:La/a/a/a/KC_f;

    invoke-static {v1, v0}, La/a/a/c/KC_a;->a(Ljava/lang/Object;Ljava/lang/StringBuilder;)V

    .line 795
    const-string v1, "}}"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 796
    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method
